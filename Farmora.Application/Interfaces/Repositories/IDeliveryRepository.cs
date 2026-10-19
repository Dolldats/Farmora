using Farmora.Domain.Entities;

namespace Farmora.Application.Interfaces.Repositories;

public interface IDeliveryRepository
{
    Task<Delivery?> GetByIdAsync(Guid deliveryId, CancellationToken cancellationToken);
    Task<Delivery?> GetByOrderAsync(Guid orderId, CancellationToken cancellationToken);
    Task AddAsync(Delivery delivery, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
