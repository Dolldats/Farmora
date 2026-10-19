using Farmora.Application.DTO.Delivery;

namespace Farmora.Application.Interfaces.Services;

public interface IDeliveryService
{
    Task<DeliveryResponse?> GetByOrderAsync(Guid customerId, Guid orderId, CancellationToken cancellationToken);
    Task<DeliveryResponse?> UpdateStatusAsync(Guid orderId, UpdateDeliveryDto dto, CancellationToken cancellationToken);
}
