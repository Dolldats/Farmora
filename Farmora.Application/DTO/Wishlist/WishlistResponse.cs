namespace Farmora.Application.DTO.Wishlist;

public sealed class WishlistResponse
{
    public Guid Id { get; init; }
    public Guid CustomerId { get; init; }
    public Guid ProductId { get; init; }
    public DateTime CreatedAtUtc { get; init; }
}
