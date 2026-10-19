using Farmora.Domain.Enums;

namespace Farmora.Application.DTO.Delivery;

public sealed class UpdateDeliveryDto
{
    public DeliveryStatus Status { get; init; }
    public string? TrackingNumber { get; init; }
    public string? Note { get; init; }
}
