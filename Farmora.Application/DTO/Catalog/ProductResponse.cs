using Farmora.Domain.Enums;

namespace Farmora.Application.DTO.Catalog;

public sealed class ProductResponse
{
    public Guid Id { get; init; }
    public Guid FarmerId { get; init; }
    public string? FarmerName { get; init; }
    public Guid CategoryId { get; init; }
    public string CategorySlug { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public decimal Price { get; init; }
    public ProductUnit Unit { get; init; }
    public decimal Stock { get; init; }
    public ProductStatus Status { get; init; }
    public bool IsAvailable { get; init; }
    public double? AvgRating { get; init; }
    public IReadOnlyList<ProductImageResponse> Images { get; init; } = [];
    public DateTime CreatedAtUtc { get; init; }
}
