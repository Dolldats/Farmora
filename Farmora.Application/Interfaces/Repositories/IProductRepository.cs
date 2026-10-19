using Farmora.Application.DTO.Catalog;
using Farmora.Domain.Entities;

namespace Farmora.Application.Interfaces.Repositories;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<(IReadOnlyList<Product> Items, int Total)> SearchAsync(
        ProductSearchParams query, CancellationToken cancellationToken);
    Task AddAsync(Product product, CancellationToken cancellationToken);
    Task RemoveAsync(Product product, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
