using Farmora.Application.DTO.Wishlist;
using Farmora.Application.Interfaces.Services;
using Farmora.Domain.Entities;
using Farmora.Infastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Farmora.Infastructure.Services;

public sealed class WishlistService(FarmoraDbContext db) : IWishlistService
{
    public async Task<IReadOnlyList<WishlistResponse>> ListAsync(Guid customerId, CancellationToken cancellationToken)
    {
        var customer = await RequireCustomerAsync(customerId, cancellationToken);
        return await db.Set<WishlistItem>()
            .AsNoTracking()
            .Where(w => w.CustomerId == customer.Id)
            .OrderByDescending(w => w.CreatedAtUtc)
            .Select(w => new WishlistResponse
            {
                Id = w.Id,
                CustomerId = w.CustomerId,
                ProductId = w.ProductId,
                CreatedAtUtc = w.CreatedAtUtc,
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<WishlistResponse> AddAsync(Guid customerId, AddWishlistDto dto, CancellationToken cancellationToken)
    {
        var customer = await RequireCustomerAsync(customerId, cancellationToken);
        var productExists = await db.Set<Product>().AsNoTracking().AnyAsync(p => p.Id == dto.ProductId, cancellationToken);
        if (!productExists)
            throw new InvalidOperationException("Product not found.");

        var existing = await db.Set<WishlistItem>()
            .FirstOrDefaultAsync(w => w.CustomerId == customer.Id && w.ProductId == dto.ProductId, cancellationToken);
        if (existing is not null)
            return Map(existing);

        var item = new WishlistItem(customer.Id, dto.ProductId);
        await db.Set<WishlistItem>().AddAsync(item, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return Map(item);
    }

    public async Task<bool> RemoveAsync(Guid customerId, Guid productId, CancellationToken cancellationToken)
    {
        var customer = await RequireCustomerAsync(customerId, cancellationToken);
        var existing = await db.Set<WishlistItem>()
            .FirstOrDefaultAsync(w => w.CustomerId == customer.Id && w.ProductId == productId, cancellationToken);
        if (existing is null) return false;
        db.Set<WishlistItem>().Remove(existing);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<Customer> RequireCustomerAsync(Guid idOrUserId, CancellationToken ct) =>
        await db.Set<Customer>().FirstOrDefaultAsync(c => c.Id == idOrUserId, ct)
        ?? await db.Set<Customer>().FirstOrDefaultAsync(c => c.UserId == idOrUserId, ct)
        ?? throw new InvalidOperationException("Customer profile required.");

    private static WishlistResponse Map(WishlistItem w) => new()
    {
        Id = w.Id,
        CustomerId = w.CustomerId,
        ProductId = w.ProductId,
        CreatedAtUtc = w.CreatedAtUtc,
    };
}
