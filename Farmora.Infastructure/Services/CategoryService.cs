using Farmora.Application.DTO.Catalog;
using Farmora.Application.Interfaces.Services;
using Farmora.Domain.Entities;
using Farmora.Infastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Farmora.Infastructure.Services;

public sealed class CategoryService(FarmoraDbContext db) : ICategoryService
{
    public async Task<(IReadOnlyList<CategoryResponse> Items, int Total)> GetPagedAsync(
        int page, int pageSize, bool? onlyActive, CancellationToken cancellationToken)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var query = db.Set<Category>().AsNoTracking().AsQueryable();
        if (onlyActive.HasValue)
            query = query.Where(c => c.IsActive == onlyActive.Value);
        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderBy(c => c.SortOrder).ThenBy(c => c.Name)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(c => new CategoryResponse
            {
                Id = c.Id,
                Name = c.Name,
                Slug = c.Slug,
                IsActive = c.IsActive,
                SortOrder = c.SortOrder,
                ParentCategoryId = c.ParentCategoryId,
            })
            .ToListAsync(cancellationToken);
        return (items, total);
    }

    public async Task<CategoryResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var c = await db.Set<Category>().AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        return c is null ? null : Map(c);
    }

    public async Task<CategoryResponse?> GetBySlugAsync(string slug, CancellationToken cancellationToken)
    {
        var normalized = (slug ?? string.Empty).Trim().ToLowerInvariant();
        var c = await db.Set<Category>().AsNoTracking().SingleOrDefaultAsync(x => x.Slug == normalized, cancellationToken);
        return c is null ? null : Map(c);
    }

    public async Task<CategoryResponse> CreateAsync(CreateCategoryDto dto, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException("Category name is required.");
        var slug = string.IsNullOrWhiteSpace(dto.Slug)
            ? Slugify(dto.Name)
            : dto.Slug.Trim().ToLowerInvariant();
        slug = await UniqueSlugAsync(slug, null, cancellationToken);

        var category = new Category(dto.Name.Trim(), slug, dto.ParentCategoryId);
        db.Set<Category>().Add(category);
        await db.SaveChangesAsync(cancellationToken);

        db.Entry(category).Property(c => c.SortOrder).CurrentValue = dto.SortOrder;
        category.MarkUpdated();
        await db.SaveChangesAsync(cancellationToken);
        return Map(category);
    }

    public async Task<CategoryResponse?> UpdateAsync(Guid id, UpdateCategoryDto dto, CancellationToken cancellationToken)
    {
        var category = await db.Set<Category>().SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (category is null)
            return null;

        var entry = db.Entry(category);
        if (!string.IsNullOrWhiteSpace(dto.Name))
            entry.Property(c => c.Name).CurrentValue = dto.Name.Trim();
        if (dto.Slug is not null)
        {
            var slug = string.IsNullOrWhiteSpace(dto.Slug) ? Slugify(entry.Property(c => c.Name).CurrentValue ?? category.Name) : dto.Slug.Trim().ToLowerInvariant();
            entry.Property(c => c.Slug).CurrentValue = await UniqueSlugAsync(slug, id, cancellationToken);
        }
        entry.Property(c => c.ParentCategoryId).CurrentValue = dto.ParentCategoryId;
        entry.Property(c => c.SortOrder).CurrentValue = dto.SortOrder;
        entry.Property(c => c.IsActive).CurrentValue = dto.IsActive;
        category.MarkUpdated();
        await db.SaveChangesAsync(cancellationToken);
        return Map(category);
    }

    public async Task<bool> DeactivateAsync(Guid id, CancellationToken cancellationToken)
    {
        var category = await db.Set<Category>().SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (category is null)
            return false;
        db.Entry(category).Property(c => c.IsActive).CurrentValue = false;
        category.MarkUpdated();
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<string> UniqueSlugAsync(string baseSlug, Guid? excludeId, CancellationToken ct)
    {
        var slug = baseSlug;
        var counter = 2;
        while (await db.Set<Category>().AnyAsync(
            c => c.Slug == slug && (!excludeId.HasValue || c.Id != excludeId.Value), ct))
        {
            slug = $"{baseSlug}-{counter++}";
        }
        return slug;
    }

    private static string Slugify(string name)
    {
        var chars = name.Trim().ToLowerInvariant()
            .Select(c => char.IsLetterOrDigit(c) ? c : '-')
            .ToArray();
        var collapsed = new string(chars);
        while (collapsed.Contains("--", StringComparison.Ordinal))
            collapsed = collapsed.Replace("--", "-", StringComparison.Ordinal);
        return collapsed.Trim('-');
    }

    private static CategoryResponse Map(Category c) => new()
    {
        Id = c.Id,
        Name = c.Name,
        Slug = c.Slug,
        IsActive = c.IsActive,
        SortOrder = c.SortOrder,
        ParentCategoryId = c.ParentCategoryId,
    };
}
