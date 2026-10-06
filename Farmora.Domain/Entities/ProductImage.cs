using Farmora.Domain.Common;
namespace Farmora.Domain.Entities;
public sealed class ProductImage : Entity
{
    private ProductImage()
    { 
    }

    public ProductImage(Guid productId, string storageKey, string url, int sortOrder, bool isPrimary = false) 
    { 
        ProductId = productId; 
        StorageKey = storageKey; 
        Url = url; 
        SortOrder = sortOrder; 
        IsPrimary = isPrimary; 
    }

    public Guid ProductId { get; private set; }
    public string StorageKey { get; private set; } = null!;
    public string Url { get; private set; } = null!;
    public int SortOrder { get; private set; }
    public bool IsPrimary { get; private set; }
    public bool IsApproved { get; private set; }
    public Product Product { get; private set; } = null!;
}
