using Farmora.Domain.Entities;

namespace Farmora.Application.Interfaces.Repositories;

public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Category?> GetBySlugAsync(string slug, CancellationToken cancellationToken);
    Task<(IReadOnlyList<Category> Items, int Total)> GetPagedAsync(
        int page, int pageSize, bool? onlyActive, CancellationToken cancellationToken);
    Task AddAsync(Category category, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
