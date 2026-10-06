using Farmora.Domain.Common;
namespace Farmora.Domain.Entities;
public sealed class Category : AuditableEntity
{
    private readonly List<Category> _children = [];
    private Category() { }
    public Category(string name, string slug, Guid? parentCategoryId = null) 
    { 
        Name = name.Trim();
        Slug = slug.Trim().ToLowerInvariant(); 
        ParentCategoryId = parentCategoryId; 
    }

    public string Name { get; private set; } = null!;
    public string Slug { get; private set; } = null!;
    public bool IsActive { get; private set; } = true;
    public int SortOrder { get; private set; }
    public Guid? ParentCategoryId { get; private set; }
    public Category? ParentCategory { get; private set; }
    public IReadOnlyCollection<Category> Children => _children;
}
