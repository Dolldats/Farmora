using Farmora.Application.Interfaces.Repositories;
using Farmora.Domain.Entities;
using Farmora.Infastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Farmora.Infastructure.Repositories;

public sealed class CategoryRepository(FarmoraDbContext db) : ICategoryRepository
{
    public Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return db.Categories.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<Category?> GetBySlugAsync(string slug, CancellationToken cancellationToken)
    {
        return db.Categories.SingleOrDefaultAsync(x => x.Slug == slug, cancellationToken);
    }

    public async Task<(IReadOnlyList<Category> Items, int Total)> GetPagedAsync(
        int page, int pageSize, bool? onlyActive, CancellationToken cancellationToken)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 20 : pageSize > 100 ? 100 : pageSize;

        var query = db.Categories.AsNoTracking().AsQueryable();
        if (onlyActive.HasValue)
            query = query.Where(x => x.IsActive == onlyActive.Value);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public Task AddAsync(Category category, CancellationToken cancellationToken)
    {
        return db.Categories.AddAsync(category, cancellationToken).AsTask();
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return db.SaveChangesAsync(cancellationToken);
    }
}
