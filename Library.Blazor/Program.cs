using Library.Application.Members;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Library.Blazor.Components;
using Library.Blazor.Components.Account;
using Library.Blazor.Data;
using Library.Blazor.Data.Repositories.Books;
using Library.Blazor.Data.Repositories.Loans;
using Library.Blazor.Data.Repositories.Members;
using Library.Blazor.Data.Repositories.Reservations;
using Library.Domain.Books;
using Library.Domain.Loans;
using Library.Domain.Members;
using Library.Domain.Reservations;
using MediatR;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                       throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = true;
        options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders()
    ;

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

// UI Stuff
builder.Services.AddMudServices();
builder.Services.AddMediatR(cfg => 
    cfg.RegisterServicesFromAssembly(typeof(CreateMemberHandler).Assembly));

//App Specific
builder.Services.AddScoped<IMemberRepository, MemberRepository>();
builder.Services.AddScoped<IBookRepository, BookRepository>();
builder.Services.AddScoped<ILoanPolicyRepository, LoanPolicyRepository>();
builder.Services.AddScoped<ILoanRepository, LoanRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IReservationRepository, ReservationRepository>();
var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    // Seed initial user and roles
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

    foreach (var role in new[] { "Admin", "Member" })
    {
        if (!await roleManager.RoleExistsAsync(role)) 
            await roleManager.CreateAsync(new IdentityRole(role));
    }

    var adminEmail = "admin@gmail.com";
    var admin = await userManager.FindByEmailAsync(adminEmail);
    if (admin is null)
    {
        admin = new ApplicationUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
        await userManager.CreateAsync(admin, "Password123!");
        await userManager.AddToRoleAsync(admin, "Admin");
        await mediator.Send(new CreateMemberCommand(Guid.Parse(admin.Id), "Admin Name", adminEmail));
    }

    foreach (var demoUser in new[] { "user1", "user2", "user3" })
    {
        var userExists = await userManager.FindByEmailAsync($"{demoUser}@gmail.com");
        if (userExists is not null) continue;
        var user = new ApplicationUser { UserName = $"{demoUser}@gmail.com", Email = $"{demoUser}@gmail.com",  EmailConfirmed = true };
        await userManager.CreateAsync(user, "Password123!");
        await userManager.AddToRoleAsync(user, "Member");
        await mediator.Send(new CreateMemberCommand(Guid.Parse(user.Id), demoUser + "Name", user.Email));
    }
    
    //Seed initial loan policy
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    if (!context.LoanPolicies.Any())
    {
        await context.LoanPolicies.AddAsync(new LoanPolicy(14, 1, DateTimeOffset.UtcNow));
        await context.SaveChangesAsync();
    }

    //Seed demo catalogue, loans, reservations and payments
    if (!context.Books.Any())
    {
        var member1 = await context.Members.SingleAsync(m => m.Email == "user1@gmail.com");
        var member2 = await context.Members.SingleAsync(m => m.Email == "user2@gmail.com");
        var member3 = await context.Members.SingleAsync(m => m.Email == "user3@gmail.com");

        var now = DateTimeOffset.UtcNow;

        // Clean Code -- checked out by user1, overdue with an unpaid late fee.
        var cleanCode = new Book("Clean Code", "Robert C. Martin", "Technology");
        var cleanCodeCopy = cleanCode.AddCopy();
        cleanCodeCopy.SetOnLoan();
        var overdueLoan = new Loan(cleanCode.Id, cleanCodeCopy.Id, member1.Id, now.AddDays(-30), 14, 1);
        overdueLoan.SetOverdue(now);
        await context.Books.AddAsync(cleanCode);
        await context.Loans.AddAsync(overdueLoan);

        // The Pragmatic Programmer -- on loan to user3 (active, not due yet); user2 has reserved it and is still waiting.
        var pragProg = new Book("The Pragmatic Programmer", "Andrew Hunt & David Thomas", "Technology");
        var pragProgCopy = pragProg.AddCopy();
        pragProgCopy.SetOnLoan();
        var activeLoan = new Loan(pragProg.Id, pragProgCopy.Id, member3.Id, now.AddDays(-5), 14, 1);
        var pendingReservation = new Reservation(pragProg.Id, member2.Id, now.AddDays(-2));
        await context.Books.AddAsync(pragProg);
        await context.Loans.AddAsync(activeLoan);
        await context.Reservations.AddAsync(pendingReservation);

        // Domain-Driven Design -- returned late by user3, late fee paid in full (payment history).
        var ddd = new Book("Domain-Driven Design", "Eric Evans", "Technology");
        var dddCopy = ddd.AddCopy();
        dddCopy.SetOnLoan();
        var returnedLoan = new Loan(ddd.Id, dddCopy.Id, member3.Id, now.AddDays(-20), 14, 1);
        var lateFee = returnedLoan.Return(now.AddDays(-3));
        dddCopy.SetAvailable();
        var payment = new Payment(member3.Id, lateFee, "Late fee for Domain-Driven Design", now.AddDays(-3), returnedLoan.Id);
        await context.Books.AddAsync(ddd);
        await context.Loans.AddAsync(returnedLoan);
        await context.Payments.AddAsync(payment);

        // Design Patterns and 1984 -- untouched catalogue stock for browsing/searching/borrowing demos.
        var designPatterns = new Book("Design Patterns", "Gamma, Helm, Johnson, Vlissides", "Technology");
        designPatterns.AddCopy();
        designPatterns.AddCopy();
        var nineteenEightyFour = new Book("1984", "George Orwell", "Fiction");
        nineteenEightyFour.AddCopy();
        nineteenEightyFour.AddCopy();
        nineteenEightyFour.AddCopy();
        await context.Books.AddAsync(designPatterns);
        await context.Books.AddAsync(nineteenEightyFour);

        member1.AddInterest("Software Engineering");
        member2.AddInterest("Fiction");
        member3.AddInterest("Design Patterns");

        await context.SaveChangesAsync();
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.Run();