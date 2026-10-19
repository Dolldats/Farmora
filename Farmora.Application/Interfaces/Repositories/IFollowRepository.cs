using Farmora.Domain.Entities;

namespace Farmora.Application.Interfaces.Repositories;

public interface IFollowRepository
{
    Task<FarmerFollow?> GetFollowAsync(Guid customerId, Guid farmerId, CancellationToken cancellationToken);
    Task<ProductFavorite?> GetFavoriteAsync(Guid customerId, Guid productId, CancellationToken cancellationToken);
    Task<IReadOnlyList<FarmerFollow>> GetFollowsByCustomerAsync(Guid customerId, CancellationToken cancellationToken);
    Task<IReadOnlyList<ProductFavorite>> GetFavoritesByCustomerAsync(Guid customerId, CancellationToken cancellationToken);
    Task<int> CountFollowersAsync(Guid farmerId, CancellationToken cancellationToken);
    Task AddFollowAsync(FarmerFollow follow, CancellationToken cancellationToken);
    Task RemoveFollowAsync(FarmerFollow follow, CancellationToken cancellationToken);
    Task AddFavoriteAsync(ProductFavorite favorite, CancellationToken cancellationToken);
    Task RemoveFavoriteAsync(ProductFavorite favorite, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
