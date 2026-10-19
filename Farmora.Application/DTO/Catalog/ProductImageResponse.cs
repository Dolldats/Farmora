namespace Farmora.Application.DTO.Catalog;

public sealed class ProductImageResponse
{
    public Guid Id { get; init; }
    public Guid ProductId { get; init; }
    public string StorageKey { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;
    public int SortOrder { get; init; }
    public bool IsPrimary { get; init; }
    public bool IsApproved { get; init; }
}
