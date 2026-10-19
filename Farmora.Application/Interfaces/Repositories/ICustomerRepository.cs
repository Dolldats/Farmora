using Farmora.Domain.Entities;

namespace Farmora.Application.Interfaces.Repositories;

public interface ICustomerRepository
{
    Task<Customer?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(Customer customer, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
