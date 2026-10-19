using Farmora.Application.DTO.Catalog;
using Farmora.Application.Interfaces.Services;
using Farmora.Domain.Entities;
using Farmora.Domain.Enums;
using Farmora.Infastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Farmora.Infastructure.Services;

public sealed class ProductService(FarmoraDbContext db) : IProductService
{
    public async Task<(IReadOnlyList<ProductResponse> Items, int Total)> SearchAsync(
        ProductSearchParams query, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var baseQuery = db.Set<Product>().AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            var kw = query.Keyword.Trim().ToLowerInvariant();
            baseQuery = baseQuery.Where(p => p.Name.ToLower().Contains(kw) || (p.Description != null && p.Description.ToLower().Contains(kw)));
        }
        if (query.CategoryId.HasValue)
            baseQuery = baseQuery.Where(p => p.CategoryId == query.CategoryId.Value);
        if (query.FarmerId.HasValue)
            baseQuery = baseQuery.Where(p => p.FarmerId == query.FarmerId.Value);
        if (query.MinPrice.HasValue)
            baseQuery = baseQuery.Where(p => p.Price >= query.MinPrice.Value);
        if (query.MaxPrice.HasValue)
            baseQuery = baseQuery.Where(p => p.Price <= query.MaxPrice.Value);
        if (query.OnlyAvailable.HasValue && query.OnlyAvailable.Value)
            baseQuery = baseQuery.Where(p => p.IsAvailable && p.Status == ProductStatus.Published);
        if (query.MinRating.HasValue)
        {
            var min = query.MinRating.Value;
            baseQuery = baseQuery.Where(p =>
                (db.Set<Review>().Where(r => r.ProductId == p.Id && r.Status == ReviewStatus.Approved)
                    .Select(r => (double?)r.Rating).Average() ?? 0) >= min);
        }

        baseQuery = query.Sort switch
        {
            ProductSort.PriceAsc => baseQuery.OrderBy(p => p.Price),
            ProductSort.PriceDesc => baseQuery.OrderByDescending(p => p.Price),
            ProductSort.Rating => baseQuery.OrderByDescending(p =>
                db.Set<Review>().Where(r => r.ProductId == p.Id && r.Status == ReviewStatus.Approved)
                    .Select(r => (double?)r.Rating).Average() ?? 0),
            _ => baseQuery.OrderByDescending(p => p.CreatedAtUtc),
        };

        var total = await baseQuery.CountAsync(cancellationToken);
        var products = await baseQuery.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        var items = await MapManyAsync(products, cancellationToken);
        return (items, total);
    }

    public async Task<ProductResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var product = await db.Set<Product>().AsNoTracking().SingleOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (product is null)
            return null;
        return (await MapManyAsync([product], cancellationToken))[0];
    }

    public async Task<ProductResponse> CreateAsync(Guid farmerId, CreateProductDto dto, CancellationToken cancellationToken)
    {
        var farmer = await ResolveFarmerAsync(farmerId, cancellationToken)
            ?? throw new KeyNotFoundException("Farmer not found.");
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException("Product name is required.");
        var categoryExists = await db.Set<Category>().AnyAsync(c => c.Id == dto.CategoryId, cancellationToken);
        if (!categoryExists)
            throw new KeyNotFoundException("Category not found.");

        var product = new Product(farmer.Id, dto.CategoryId, dto.Name.Trim(), dto.Price, dto.Unit, dto.Stock);
        db.Set<Product>().Add(product);
        await db.SaveChangesAsync(cancellationToken);

        var entry = db.Entry(product);
        entry.Property(p => p.Description).CurrentValue = dto.Description?.Trim();
        product.MarkUpdated();
        await db.SaveChangesAsync(cancellationToken);

        return (await MapManyAsync([product], cancellationToken))[0];
    }

    public async Task<ProductResponse?> UpdateAsync(Guid farmerId, Guid productId, UpdateProductDto dto, CancellationToken cancellationToken)
    {
        var farmer = await ResolveFarmerAsync(farmerId, cancellationToken);
        var product = await db.Set<Product>().SingleOrDefaultAsync(p => p.Id == productId, cancellationToken);
        if (product is null || farmer is null || product.FarmerId != farmer.Id)
            return null;

        var entry = db.Entry(product);
        if (dto.CategoryId.HasValue)
        {
            var categoryExists = await db.Set<Category>().AnyAsync(c => c.Id == dto.CategoryId.Value, cancellationToken);
            if (!categoryExists)
                throw new KeyNotFoundException("Category not found.");
            entry.Property(p => p.CategoryId).CurrentValue = dto.CategoryId.Value;
        }
        if (dto.Name is not null)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("Product name cannot be empty.");
            entry.Property(p => p.Name).CurrentValue = dto.Name.Trim();
        }
        if (dto.Description is not null)
            entry.Property(p => p.Description).CurrentValue = dto.Description?.Trim();
        if (dto.Price.HasValue)
            product.SetPrice(dto.Price.Value);
        if (dto.Unit.HasValue)
            entry.Property(p => p.Unit).CurrentValue = dto.Unit.Value;
        if (dto.Stock.HasValue)
            product.SetStock(dto.Stock.Value);
        if (dto.IsAvailable.HasValue)
            entry.Property(p => p.IsAvailable).CurrentValue = dto.IsAvailable.Value;
        product.MarkUpdated();
        await db.SaveChangesAsync(cancellationToken);
        return (await MapManyAsync([product], cancellationToken))[0];
    }

    public async Task<bool> DeleteAsync(Guid farmerId, Guid productId, CancellationToken cancellationToken)
    {
        var farmer = await ResolveFarmerAsync(farmerId, cancellationToken);
        var product = await db.Set<Product>().SingleOrDefaultAsync(p => p.Id == productId, cancellationToken);
        if (product is null || farmer is null || product.FarmerId != farmer.Id)
            return false;
        db.Set<Product>().Remove(product);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<ProductResponse?> SetStatusAsync(Guid productId, ProductStatus status, CancellationToken cancellationToken)
    {
        var product = await db.Set<Product>().SingleOrDefaultAsync(p => p.Id == productId, cancellationToken);
        if (product is null)
            return null;

        if (status == ProductStatus.Published)
        {
            var farmer = await db.Set<Farmer>().SingleOrDefaultAsync(f => f.Id == product.FarmerId, cancellationToken);
            if (farmer is null || farmer.VerificationStatus != FarmerVerificationStatus.Approved)
                status = ProductStatus.PendingModeration;
        }

        product.SetStatus(status);
        await db.SaveChangesAsync(cancellationToken);
        return (await MapManyAsync([product], cancellationToken))[0];
    }

    public async Task<ProductResponse?> AdjustStockAsync(Guid farmerId, Guid productId, decimal delta, CancellationToken cancellationToken)
    {
        var farmer = await ResolveFarmerAsync(farmerId, cancellationToken);
        var product = await db.Set<Product>().SingleOrDefaultAsync(p => p.Id == productId, cancellationToken);
        if (product is null || farmer is null || product.FarmerId != farmer.Id)
            return null;

        var newStock = product.Stock + delta;
        if (newStock < 0)
            throw new InvalidOperationException("Insufficient stock.");
        product.SetStock(newStock);

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new InvalidOperationException("Product was modified concurrently. Please retry.", ex);
        }
        return (await MapManyAsync([product], cancellationToken))[0];
    }

    private async Task<Farmer?> ResolveFarmerAsync(Guid idOrUserId, CancellationToken ct) =>
        await db.Set<Farmer>().SingleOrDefaultAsync(f => f.Id == idOrUserId, ct)
        ?? await db.Set<Farmer>().SingleOrDefaultAsync(f => f.UserId == idOrUserId, ct);

    private async Task<IReadOnlyList<ProductResponse>> MapManyAsync(IReadOnlyList<Product> products, CancellationToken ct)
    {
        if (products.Count == 0)
            return [];
        var ids = products.Select(p => p.Id).ToList();
        var farmerIds = products.Select(p => p.FarmerId).Distinct().ToList();
        var categoryIds = products.Select(p => p.CategoryId).Distinct().ToList();

        var farmers = await db.Set<Farmer>().AsNoTracking()
            .Where(f => farmerIds.Contains(f.Id)).ToDictionaryAsync(f => f.Id, f => f.FarmName, ct);
        var categories = await db.Set<Category>().AsNoTracking()
            .Where(c => categoryIds.Contains(c.Id)).ToDictionaryAsync(c => c.Id, c => c.Slug, ct);
        var images = await db.Set<ProductImage>().AsNoTracking()
            .Where(i => ids.Contains(i.ProductId))
            .OrderBy(i => i.SortOrder)
            .ToListAsync(ct);
        var ratings = await db.Set<Review>().AsNoTracking()
            .Where(r => ids.Contains(r.ProductId) && r.Status == ReviewStatus.Approved)
            .GroupBy(r => r.ProductId)
            .Select(g => new { ProductId = g.Key, Avg = g.Average(r => (double?)r.Rating) ?? 0 })
            .ToDictionaryAsync(x => x.ProductId, x => x.Avg, ct);

        return products.Select(p => new ProductResponse
        {
            Id = p.Id,
            FarmerId = p.FarmerId,
            FarmerName = farmers.TryGetValue(p.FarmerId, out var fn) ? fn : null,
            CategoryId = p.CategoryId,
            CategorySlug = categories.TryGetValue(p.CategoryId, out var slug) ? slug : string.Empty,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            Unit = p.Unit,
            Stock = p.Stock,
            Status = p.Status,
            IsAvailable = p.IsAvailable,
            AvgRating = ratings.TryGetValue(p.Id, out var avg) ? avg : null,
            Images = images.Where(i => i.ProductId == p.Id).Select(i => new ProductImageResponse
            {
                Id = i.Id,
                ProductId = i.ProductId,
                StorageKey = i.StorageKey,
                Url = i.Url,
                SortOrder = i.SortOrder,
                IsPrimary = i.IsPrimary,
                IsApproved = i.IsApproved,
            }).ToList(),
            CreatedAtUtc = p.CreatedAtUtc,
        }).ToList();
    }
}
