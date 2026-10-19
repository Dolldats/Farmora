using Farmora.Domain.Enums;

namespace Farmora.Application.DTO.Orders;

public class QuoteRequest
{
    public Guid? AddressId { get; init; }
    public string? PromoCode { get; init; }
    public DeliveryMethod DeliveryMethod { get; init; } = DeliveryMethod.OperatorDelivery;
}
