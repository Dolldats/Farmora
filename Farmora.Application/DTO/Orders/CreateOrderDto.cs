namespace Farmora.Application.DTO.Orders;

public sealed class CreateOrderDto : QuoteRequest
{
    public string IdempotencyKey { get; init; } = string.Empty;
}
