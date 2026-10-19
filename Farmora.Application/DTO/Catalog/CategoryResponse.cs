namespace Farmora.Application.DTO.Catalog;

public sealed class CategoryResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public int SortOrder { get; init; }
    public Guid? ParentCategoryId { get; init; }
}
