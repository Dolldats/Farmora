namespace Farmora.Application.DTO.Catalog;

public sealed class CreateCategoryDto
{
    public string Name { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public int SortOrder { get; set; }
}
