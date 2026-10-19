using Farmora.Application.DTO.Wishlist;

namespace Farmora.Application.Interfaces.Services;

public interface IWishlistService
{
    Task<IReadOnlyList<WishlistResponse>> ListAsync(Guid customerId, CancellationToken cancellationToken);
    Task<WishlistResponse> AddAsync(Guid customerId, AddWishlistDto dto, CancellationToken cancellationToken);
    Task<bool> RemoveAsync(Guid customerId, Guid productId, CancellationToken cancellationToken);
}
