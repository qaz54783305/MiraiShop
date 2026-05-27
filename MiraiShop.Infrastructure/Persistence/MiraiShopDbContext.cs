using Microsoft.EntityFrameworkCore;
using MiraiShop.Domain.Entities;

namespace MiraiShop.Infrastructure.Persistence;

public class MiraiShopDbContext : DbContext
{
    public MiraiShopDbContext(DbContextOptions<MiraiShopDbContext> options)
        : base(options) { }

    public DbSet<Member> Members => Set<Member>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Member>(entity =>
        {
            entity.ToTable("Member");

            entity.HasKey(m => m.Id);
            entity.Property(m => m.Id)
                  .ValueGeneratedNever();

            entity.Property(m => m.Name)
                  .IsRequired();

            entity.Property(m => m.Email)
                  .HasMaxLength(256)
                  .IsRequired();

            entity.HasIndex(m => m.Email)
                  .IsUnique();

            entity.Property(m => m.PasswordHash)
                  .IsRequired();

            entity.Property(m => m.MailingAddress)
                  .IsRequired();

            entity.Property(m => m.ResidentialAddress);

            entity.Property(m => m.CreatedAt)
                  .IsRequired();
        });
    }
}
