using Farmora.Application.Interfaces.Repositories;
using Farmora.Domain.Entities;
using Farmora.Infastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Farmora.Infastructure.Repositories;

public sealed class FollowRepository(FarmoraDbContext db) : IFollowRepository
{
    public Task<FarmerFollow?> GetFollowAsync(Guid customerId, Guid farmerId, CancellationToken cancellationToken) =>
        db.Set<FarmerFollow>().SingleOrDefaultAsync(
            f => f.CustomerId == customerId && f.FarmerId == farmerId, cancellationToken);

    public Task<ProductFavorite?> GetFavoriteAsync(Guid customerId, Guid productId, CancellationToken cancellationToken) =>
        db.Set<ProductFavorite>().SingleOrDefaultAsync(
            f => f.CustomerId == customerId && f.ProductId == productId, cancellationToken);

    public async Task<IReadOnlyList<FarmerFollow>> GetFollowsByCustomerAsync(Guid customerId, CancellationToken cancellationToken) =>
        await db.Set<FarmerFollow>().AsNoTracking()
            .Where(f => f.CustomerId == customerId)
            .OrderByDescending(f => f.CreatedAtUtc)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<ProductFavorite>> GetFavoritesByCustomerAsync(Guid customerId, CancellationToken cancellationToken) =>
        await db.Set<ProductFavorite>().AsNoTracking()
            .Where(f => f.CustomerId == customerId)
            .OrderByDescending(f => f.CreatedAtUtc)
            .ToListAsync(cancellationToken);

    public Task<int> CountFollowersAsync(Guid farmerId, CancellationToken cancellationToken) =>
        db.Set<FarmerFollow>().CountAsync(f => f.FarmerId == farmerId, cancellationToken);

    public Task AddFollowAsync(FarmerFollow follow, CancellationToken cancellationToken) =>
        db.Set<FarmerFollow>().AddAsync(follow, cancellationToken).AsTask();

    public Task RemoveFollowAsync(FarmerFollow follow, CancellationToken cancellationToken)
    {
        db.Set<FarmerFollow>().Remove(follow);
        return Task.CompletedTask;
    }

    public Task AddFavoriteAsync(ProductFavorite favorite, CancellationToken cancellationToken) =>
        db.Set<ProductFavorite>().AddAsync(favorite, cancellationToken).AsTask();

    public Task RemoveFavoriteAsync(ProductFavorite favorite, CancellationToken cancellationToken)
    {
        db.Set<ProductFavorite>().Remove(favorite);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        db.SaveChangesAsync(cancellationToken);
}
