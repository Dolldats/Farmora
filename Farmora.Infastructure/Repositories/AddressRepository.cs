using Farmora.Application.Interfaces.Repositories;
using Farmora.Domain.Entities;
using Farmora.Infastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Farmora.Infastructure.Repositories;

public sealed class AddressRepository(FarmoraDbContext db) : IAddressRepository
{
    public Task<Address?> GetByIdAsync(Guid addressId, CancellationToken cancellationToken) =>
        db.Set<Address>().FirstOrDefaultAsync(a => a.Id == addressId, cancellationToken);

    public async Task<IReadOnlyList<Address>> ListByCustomerAsync(Guid customerId, CancellationToken cancellationToken) =>
        await db.Set<Address>()
            .AsNoTracking()
            .Where(a => a.CustomerId == customerId)
            .OrderByDescending(a => a.IsDefault)
            .ThenBy(a => a.CreatedAtUtc)
            .ToListAsync(cancellationToken);

    public Task AddAsync(Address address, CancellationToken cancellationToken) =>
        db.Set<Address>().AddAsync(address, cancellationToken).AsTask();

    public Task RemoveAsync(Address address, CancellationToken cancellationToken)
    {
        db.Set<Address>().Remove(address);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        db.SaveChangesAsync(cancellationToken);
}
