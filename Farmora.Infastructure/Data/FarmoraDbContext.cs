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
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FarmoraDbContext).Assembly);
    }
}
