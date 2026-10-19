using Farmora.Domain.Entities;

namespace Farmora.Application.Interfaces.Repositories;

public interface IOrderRepository
{
    Task<Order?> GetByIdWithDetailsAsync(Guid orderId, CancellationToken cancellationToken);
    Task<Order?> GetByIdempotencyKeyAsync(Guid customerId, string idempotencyKey, CancellationToken cancellationToken);
    Task<(IReadOnlyList<Order> Items, int TotalCount)> ListByCustomerPagedAsync(Guid customerId, int page, int pageSize, CancellationToken cancellationToken);
    Task AddAsync(Order order, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
