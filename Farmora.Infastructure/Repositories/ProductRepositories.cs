using Farmora.Application.DTO.Catalog;
using Farmora.Application.Interfaces.Repositories;
using Farmora.Domain.Entities;
using Farmora.Infastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Farmora.Infastructure.Repositories;

public sealed class ProductRepository(FarmoraDbContext db) : IProductRepository
{
    public Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return db.Products
            .Include(x => x.Images)
            .Include(x => x.Category)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<(IReadOnlyList<Product> Items, int Total)> SearchAsync(
        ProductSearchParams query, CancellationToken cancellationToken)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize < 1 ? 20 : query.PageSize > 100 ? 100 : query.PageSize;

        var products = db.Products.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            var keyword = query.Keyword.Trim().ToLowerInvariant();
            products = products.Where(x =>
                x.Name.ToLower().Contains(keyword) ||
                (x.Description != null && x.Description.ToLower().Contains(keyword)));
        }

        if (query.CategoryId.HasValue)
            products = products.Where(x => x.CategoryId == query.CategoryId.Value);

        if (query.FarmerId.HasValue)
            products = products.Where(x => x.FarmerId == query.FarmerId.Value);

        if (query.MinPrice.HasValue)
            products = products.Where(x => x.Price >= query.MinPrice.Value);

        if (query.MaxPrice.HasValue)
            products = products.Where(x => x.Price <= query.MaxPrice.Value);

        if (query.OnlyAvailable.HasValue && query.OnlyAvailable.Value)
            products = products.Where(x => x.IsAvailable);

        products = query.Sort switch
        {
            ProductSort.PriceAsc => products.OrderBy(x => x.Price).ThenByDescending(x => x.CreatedAtUtc),
            ProductSort.PriceDesc => products.OrderByDescending(x => x.Price).ThenByDescending(x => x.CreatedAtUtc),
            _ => products.OrderByDescending(x => x.CreatedAtUtc),
        };

        var total = await products.CountAsync(cancellationToken);
        var items = await products
            .Include(x => x.Images)
            .Include(x => x.Category)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public Task AddAsync(Product product, CancellationToken cancellationToken)
    {
        return db.Products.AddAsync(product, cancellationToken).AsTask();
    }

    public Task RemoveAsync(Product product, CancellationToken cancellationToken)
    {
        db.Products.Remove(product);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return db.SaveChangesAsync(cancellationToken);
    }
}

public sealed class ProductImageRepository(FarmoraDbContext db) : IProductImageRepository
{
    public Task<ProductImage?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return db.ProductImages.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<ProductImage>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken)
    {
        return await db.ProductImages
            .AsNoTracking()
            .Where(x => x.ProductId == productId)
            .OrderBy(x => x.SortOrder)
            .ToListAsync(cancellationToken);
    }

    public Task AddAsync(ProductImage image, CancellationToken cancellationToken)
    {
        return db.ProductImages.AddAsync(image, cancellationToken).AsTask();
    }

    public Task RemoveAsync(ProductImage image, CancellationToken cancellationToken)
    {
        db.ProductImages.Remove(image);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return db.SaveChangesAsync(cancellationToken);
    }
}
