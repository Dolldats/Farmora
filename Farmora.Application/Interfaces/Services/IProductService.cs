using Farmora.Application.DTO.Catalog;
using Farmora.Domain.Enums;

namespace Farmora.Application.Interfaces.Services;

public interface IProductService
{
    Task<(IReadOnlyList<ProductResponse> Items, int Total)> SearchAsync(
        ProductSearchParams query, CancellationToken cancellationToken);

    Task<ProductResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<ProductResponse> CreateAsync(
        Guid farmerId, CreateProductDto dto, CancellationToken cancellationToken);

    Task<ProductResponse?> UpdateAsync(
        Guid farmerId, Guid productId, UpdateProductDto dto, CancellationToken cancellationToken);

    Task<bool> DeleteAsync(
        Guid farmerId, Guid productId, CancellationToken cancellationToken);

    Task<ProductResponse?> SetStatusAsync(
        Guid productId, ProductStatus status, CancellationToken cancellationToken);

    Task<ProductResponse?> AdjustStockAsync(
        Guid farmerId, Guid productId, decimal delta, CancellationToken cancellationToken);
}
