using Farmora.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Farmora.Infastructure.Data;

public sealed class FarmoraDbContext(DbContextOptions<FarmoraDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Farmer> Farmers => Set<Farmer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Email).HasMaxLength(320).IsRequired();
            entity.HasIndex(x => x.Email).IsUnique();
            entity.Property(x => x.Phone).HasMaxLength(32);
            entity.Property(x => x.PasswordHash).IsRequired();
            entity.Property(x => x.RowVersion).IsRowVersion();
            entity.HasOne(x => x.Customer).WithOne(x => x.User).HasForeignKey<Customer>(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Farmer).WithOne(x => x.User).HasForeignKey<Farmer>(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Role>().HasKey(x => x.Id);
        modelBuilder.Entity<Role>().HasIndex(x => x.Name).IsUnique();
        modelBuilder.Entity<UserRole>().HasKey(x => x.Id);
        modelBuilder.Entity<UserRole>().HasIndex(x => new { x.UserId, x.RoleId }).IsUnique();
        modelBuilder.Entity<Customer>().HasKey(x => x.Id);
        modelBuilder.Entity<Farmer>().HasKey(x => x.Id);
    }
}
