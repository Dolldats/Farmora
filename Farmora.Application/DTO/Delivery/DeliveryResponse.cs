using Farmora.Domain.Enums;

namespace Farmora.Application.DTO.Delivery;

public sealed class DeliveryResponse
{
    public Guid Id { get; init; }
    public Guid OrderId { get; init; }
    public DeliveryMethod Method { get; init; }
    public decimal Fee { get; init; }
    public string? TrackingNumber { get; init; }
    public DeliveryStatus Status { get; init; }
}
