namespace Farmora.Application.DTO.Cart;

public sealed class CartResponse
{
    public Guid Id { get; init; }
    public Guid CustomerId { get; init; }
    public bool IsActive { get; init; }
    public IReadOnlyList<CartItemResponse> Items { get; init; } = [];
}
