using Farmora.Application.Interfaces.Repositories;
using Farmora.Domain.Entities;
using Farmora.Infastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Farmora.Infastructure.Repositories;

public sealed class CustomerRepository(FarmoraDbContext db) : ICustomerRepository
{
    public Task<Customer?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return db.Customers.SingleOrDefaultAsync(x => x.UserId == userId, cancellationToken);
    }

    public Task AddAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        return db.Customers.AddAsync(customer, cancellationToken).AsTask();
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return db.SaveChangesAsync(cancellationToken);
    }
}

public sealed class FarmerRepository(FarmoraDbContext db) : IFarmerRepository
{
    public Task<Farmer?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return db.Farmers.SingleOrDefaultAsync(x => x.UserId == userId, cancellationToken);
    }

    public Task AddAsync(Farmer farmer, CancellationToken cancellationToken = default)
    {
        return db.Farmers.AddAsync(farmer, cancellationToken).AsTask();
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return db.SaveChangesAsync(cancellationToken);
    }
}

public sealed class RoleRepository(FarmoraDbContext db) : IRoleRepository
{
    public Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return db.Roles.SingleOrDefaultAsync(x => x.Name == name, cancellationToken);
    }

    public async Task<IReadOnlyList<Role>> GetByNamesAsync(IEnumerable<string> names, CancellationToken cancellationToken = default)
    {
        var nameList = names.ToList();
        return await db.Roles
            .Where(x => nameList.Contains(x.Name))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Role>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await db.Roles.AsNoTracking().OrderBy(x => x.Name).ToListAsync(cancellationToken);
    }

    public Task AddAsync(Role role, CancellationToken cancellationToken = default)
    {
        return db.Roles.AddAsync(role, cancellationToken).AsTask();
    }
}
