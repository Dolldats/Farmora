namespace Farmora.Application.DTO.Orders;

public sealed class QuoteResponse
{
    public decimal Subtotal { get; init; }
    public decimal Discount { get; init; }
    public decimal DeliveryFee { get; init; }
    public decimal Total { get; init; }
    public string Currency { get; init; } = "NGN";
}
