using Farmora.Application.DTO.Following;
using Farmora.Application.Interfaces.Services;
using Farmora.Domain.Entities;
using Farmora.Infastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Farmora.Infastructure.Services;

public sealed class FollowService(FarmoraDbContext db) : IFollowService
{
    public async Task<FollowResponse?> FollowFarmerAsync(Guid customerId, Guid farmerId, CancellationToken cancellationToken)
    {
        var customer = await ResolveCustomerAsync(customerId, cancellationToken);
        if (customer is null)
            return null;
        var farmerExists = await db.Set<Farmer>().AnyAsync(f => f.Id == farmerId, cancellationToken);
        if (!farmerExists)
            throw new KeyNotFoundException("Farmer not found.");

        var existing = await db.Set<FarmerFollow>().SingleOrDefaultAsync(
            f => f.CustomerId == customer.Id && f.FarmerId == farmerId, cancellationToken);
        if (existing is not null)
            return Map(existing);

        var follow = new FarmerFollow(customer.Id, farmerId);
        db.Set<FarmerFollow>().Add(follow);
        await db.SaveChangesAsync(cancellationToken);
        return Map(follow);
    }

    public async Task<bool> UnfollowFarmerAsync(Guid customerId, Guid farmerId, CancellationToken cancellationToken)
    {
        var customer = await ResolveCustomerAsync(customerId, cancellationToken);
        if (customer is null)
            return false;
        var existing = await db.Set<FarmerFollow>().SingleOrDefaultAsync(
            f => f.CustomerId == customer.Id && f.FarmerId == farmerId, cancellationToken);
        if (existing is null)
            return false;
        db.Set<FarmerFollow>().Remove(existing);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<FavoriteResponse?> FavoriteProductAsync(Guid customerId, Guid productId, CancellationToken cancellationToken)
    {
        var customer = await ResolveCustomerAsync(customerId, cancellationToken);
        if (customer is null)
            return null;
        var productExists = await db.Set<Product>().AnyAsync(p => p.Id == productId, cancellationToken);
        if (!productExists)
            throw new KeyNotFoundException("Product not found.");

        var existing = await db.Set<ProductFavorite>().SingleOrDefaultAsync(
            f => f.CustomerId == customer.Id && f.ProductId == productId, cancellationToken);
        if (existing is not null)
            return Map(existing);

        var favorite = new ProductFavorite(customer.Id, productId);
        db.Set<ProductFavorite>().Add(favorite);
        await db.SaveChangesAsync(cancellationToken);
        return Map(favorite);
    }

    public async Task<bool> UnfavoriteProductAsync(Guid customerId, Guid productId, CancellationToken cancellationToken)
    {
        var customer = await ResolveCustomerAsync(customerId, cancellationToken);
        if (customer is null)
            return false;
        var existing = await db.Set<ProductFavorite>().SingleOrDefaultAsync(
            f => f.CustomerId == customer.Id && f.ProductId == productId, cancellationToken);
        if (existing is null)
            return false;
        db.Set<ProductFavorite>().Remove(existing);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<IReadOnlyList<FollowResponse>> GetMyFollowsAsync(Guid customerId, CancellationToken cancellationToken)
    {
        var customer = await ResolveCustomerAsync(customerId, cancellationToken);
        if (customer is null)
            return [];
        return await db.Set<FarmerFollow>().AsNoTracking()
            .Where(f => f.CustomerId == customer.Id)
            .OrderByDescending(f => f.CreatedAtUtc)
            .Select(f => new FollowResponse
            {
                CustomerId = f.CustomerId,
                FarmerId = f.FarmerId,
                CreatedAtUtc = f.CreatedAtUtc,
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<FavoriteResponse>> GetMyFavoritesAsync(Guid customerId, CancellationToken cancellationToken)
    {
        var customer = await ResolveCustomerAsync(customerId, cancellationToken);
        if (customer is null)
            return [];
        return await db.Set<ProductFavorite>().AsNoTracking()
            .Where(f => f.CustomerId == customer.Id)
            .OrderByDescending(f => f.CreatedAtUtc)
            .Select(f => new FavoriteResponse
            {
                CustomerId = f.CustomerId,
                ProductId = f.ProductId,
                CreatedAtUtc = f.CreatedAtUtc,
            })
            .ToListAsync(cancellationToken);
    }

    private async Task<Customer?> ResolveCustomerAsync(Guid idOrUserId, CancellationToken ct) =>
        await db.Set<Customer>().SingleOrDefaultAsync(c => c.Id == idOrUserId, ct)
        ?? await db.Set<Customer>().SingleOrDefaultAsync(c => c.UserId == idOrUserId, ct);

    private static FollowResponse Map(FarmerFollow f) => new()
    {
        CustomerId = f.CustomerId,
        FarmerId = f.FarmerId,
        CreatedAtUtc = f.CreatedAtUtc,
    };

    private static FavoriteResponse Map(ProductFavorite f) => new()
    {
        CustomerId = f.CustomerId,
        ProductId = f.ProductId,
        CreatedAtUtc = f.CreatedAtUtc,
    };
}
