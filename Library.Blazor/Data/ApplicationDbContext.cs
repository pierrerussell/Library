using Library.Domain.Books;
using Library.Domain.Loans;
using Library.Domain.Members;
using Library.Domain.Reservations;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Library.Blazor.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Member> Members => Set<Member>();
    public DbSet<Book> Books => Set<Book>();
    public DbSet<LoanPolicy> LoanPolicies => Set<LoanPolicy>();
    public DbSet<Loan> Loans => Set<Loan>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Reservation> Reservations => Set<Reservation>();

    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        builder.Entity<Member>(b =>
        {
            b.ToTable("Members");
            b.HasKey(m => m.Id);
            b.Property(m => m.Name)
                .IsRequired()
                .HasMaxLength(100);
            b.Property(m => m.Email)
                .IsRequired()
                .HasMaxLength(100);
            b.Property<List<string>>("_interests")
                .HasColumnName("Interests")
                .HasConversion(
                    v => string.Join(";", v),
                    v => v.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList());
            b.Metadata.FindProperty("_interests")!
                .SetPropertyAccessMode(PropertyAccessMode.Field);
        });

        builder.Entity<Book>(b =>
        {
            b.ToTable("Books");
            b.HasKey(x => x.Id);
            b.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(255);
            b.Property(x => x.Author)
                .IsRequired()
                .HasMaxLength(255);
            b.Property(x => x.Genre)
                .IsRequired()
                .HasMaxLength(255);
            
            b.Metadata.FindNavigation(nameof(Book.Copies))!
                .SetPropertyAccessMode(PropertyAccessMode.Field);
            
            b.HasMany(x => x.Copies)
                .WithOne()
                .HasForeignKey(x => x.BookId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<BookCopy>(b =>
        {
            b.ToTable("BookCopies");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id) .ValueGeneratedNever();
            b.Property(x => x.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(255);
            b.Property(x => x.BookId)
                .IsRequired();
        });

        builder.Entity<LoanPolicy>(b =>
        {
            b.ToTable("LoanPolicies");
            b.HasKey(p => p.Id);
            b.Property(p => p.LoanPeriodDays)
                .IsRequired();
            b.Property(p => p.LateFeePerDay)
                .IsRequired()
                .HasColumnType("DECIMAL(18,2)");
            b.Property(p => p.ValidFrom)
                .HasConversion(
                    value => value.ToUnixTimeMilliseconds(),
                    value => DateTimeOffset.FromUnixTimeMilliseconds(value))
                .IsRequired();
            b.HasIndex(p => p.ValidFrom);

        });

        builder.Entity<Loan>(b =>
        {
            b.ToTable("Loans");
            b.HasKey(l => l.Id);
            b.Property(l => l.CopyId)
                .IsRequired();
            b.Property(l => l.BookId)
                .IsRequired();
            b.Property(l => l.MemberId)
                .IsRequired();
            b.Property(l => l.CheckoutDate)
                .HasConversion(
                    value => value.ToUnixTimeMilliseconds(),
                    value => DateTimeOffset.FromUnixTimeMilliseconds(value))
                .IsRequired();
            b.Property(l => l.DueDate)
                .HasConversion(
                    value => value.ToUnixTimeMilliseconds(),
                    value => DateTimeOffset.FromUnixTimeMilliseconds(value))
                .IsRequired();
            b.Property(l => l.Status)
                .IsRequired()
                .HasConversion<string>();
            b.Ignore(l => l.DomainEvents);
        });

        builder.Entity<Payment>(b =>
        {
            b.ToTable("Payments");
            b.HasKey(p => p.Id);
            b.Property(p => p.MemberId)
                .IsRequired();
            b.OwnsOne(p => p.Amount, a =>
            {
                a.Property(m => m.Amount).HasColumnName("Amount").HasColumnType("DECIMAL(10,2)");
            });
            b.Property(p => p.Description)
                .IsRequired();
            b.Property(p => p.Date)
                .HasConversion(
                    value => value.ToUnixTimeMilliseconds(),
                    value => DateTimeOffset.FromUnixTimeMilliseconds(value))
                .IsRequired();
        });

        builder.Entity<Reservation>(b =>
        {
            b.ToTable("Reservations");
            b.HasKey(r => r.Id);
            b.Property(r => r.BookId)
                .IsRequired();
            b.Property(r => r.MemberId)
                .IsRequired();
            b.Property(r => r.ReservationDate)
                .HasConversion(
                    value => value.ToUnixTimeMilliseconds(),
                    value => DateTimeOffset.FromUnixTimeMilliseconds(value))
                .IsRequired();
            b.Property(r => r.Status)
                .IsRequired()
                .HasConversion<string>();
            b.Ignore(r => r.DomainEvents);
        });

    }
    
    
}