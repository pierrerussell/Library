using Library.Domain.Books;
using Library.Domain.Members;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Library.Blazor.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Member> Members => Set<Member>();
    public DbSet<Book> Books => Set<Book>();
    
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

    }
}