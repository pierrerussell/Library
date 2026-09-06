using Library.Domain.Members;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Library.Blazor.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Member> Members => Set<Member>();
    
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
        
        
    }
}