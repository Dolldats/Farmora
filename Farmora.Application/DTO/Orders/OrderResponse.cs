using Farmora.Domain.Enums;

namespace Farmora.Application.DTO.Orders;

public sealed class OrderResponse
{
    public Guid Id { get; init; }
    public Guid CustomerId { get; init; }
    public string OrderNumber { get; init; } = string.Empty;
    public string AddressSnapshot { get; init; } = string.Empty;
    public decimal Subtotal { get; init; }
    public decimal DiscountTotal { get; init; }
    public decimal DeliveryFee { get; init; }
    public decimal Total { get; init; }
    public string Currency { get; init; } = "NGN";
    public OrderStatus Status { get; init; }
    public DeliveryStatus? DeliveryStatus { get; init; }
    public string IdempotencyKey { get; init; } = string.Empty;
    public DateTime CreatedAtUtc { get; init; }
    public IReadOnlyList<OrderItemResponse> Items { get; init; } = [];
}
