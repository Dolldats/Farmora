using Farmora.Domain.Entities;

namespace Farmora.Application.Interfaces.Repositories;

public interface IProductImageRepository
{
    Task<ProductImage?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<ProductImage>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken);
    Task AddAsync(ProductImage image, CancellationToken cancellationToken);
    Task RemoveAsync(ProductImage image, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
