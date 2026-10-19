using Farmora.Domain.Entities;

namespace Farmora.Application.Interfaces.Repositories;

public interface IAddressRepository
{
    Task<Address?> GetByIdAsync(Guid addressId, CancellationToken cancellationToken);
    Task<IReadOnlyList<Address>> ListByCustomerAsync(Guid customerId, CancellationToken cancellationToken);
    Task AddAsync(Address address, CancellationToken cancellationToken);
    Task RemoveAsync(Address address, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
