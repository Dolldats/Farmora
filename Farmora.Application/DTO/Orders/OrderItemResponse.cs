using Farmora.Domain.Enums;

namespace Farmora.Application.DTO.Orders;

public sealed class OrderItemResponse
{
    public Guid Id { get; init; }
    public Guid ProductId { get; init; }
    public Guid FarmerId { get; init; }
    public string ProductNameSnapshot { get; init; } = string.Empty;
    public decimal UnitPriceSnapshot { get; init; }
    public string UnitSnapshot { get; init; } = string.Empty;
    public decimal Quantity { get; init; }
    public decimal LineTotal { get; init; }
}
