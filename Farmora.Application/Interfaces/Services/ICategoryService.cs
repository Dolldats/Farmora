using Farmora.Application.DTO.Catalog;

namespace Farmora.Application.Interfaces.Services;

public interface ICategoryService
{
    Task<(IReadOnlyList<CategoryResponse> Items, int Total)> GetPagedAsync(
        int page, int pageSize, bool? onlyActive, CancellationToken cancellationToken);

    Task<CategoryResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<CategoryResponse?> GetBySlugAsync(string slug, CancellationToken cancellationToken);

    Task<CategoryResponse> CreateAsync(CreateCategoryDto dto, CancellationToken cancellationToken);

    Task<CategoryResponse?> UpdateAsync(Guid id, UpdateCategoryDto dto, CancellationToken cancellationToken);

    Task<bool> DeactivateAsync(Guid id, CancellationToken cancellationToken);
}
