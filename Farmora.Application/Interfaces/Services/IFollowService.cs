using Farmora.Application.DTO.Following;

namespace Farmora.Application.Interfaces.Services;

public interface IFollowService
{
    Task<FollowResponse?> FollowFarmerAsync(Guid customerId, Guid farmerId, CancellationToken cancellationToken);
    Task<bool> UnfollowFarmerAsync(Guid customerId, Guid farmerId, CancellationToken cancellationToken);
    Task<FavoriteResponse?> FavoriteProductAsync(Guid customerId, Guid productId, CancellationToken cancellationToken);
    Task<bool> UnfavoriteProductAsync(Guid customerId, Guid productId, CancellationToken cancellationToken);
    Task<IReadOnlyList<FollowResponse>> GetMyFollowsAsync(Guid customerId, CancellationToken cancellationToken);
    Task<IReadOnlyList<FavoriteResponse>> GetMyFavoritesAsync(Guid customerId, CancellationToken cancellationToken);
}
