namespace Farmora.Application.DTO.Cart;

public sealed class CartItemResponse
{
    public Guid Id { get; init; }
    public Guid ProductId { get; init; }
    public decimal Quantity { get; init; }
}
