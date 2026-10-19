namespace Farmora.Application.DTO.Cart;

public sealed class AddCartItemDto
{
    public Guid ProductId { get; init; }
    public decimal Quantity { get; init; }
}
