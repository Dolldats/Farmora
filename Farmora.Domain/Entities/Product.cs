using Farmora.Domain.Common;
using Farmora.Domain.Enums;
namespace Farmora.Domain.Entities;
public sealed class Product : AuditableEntity
{
    private readonly List<ProductImage> _images = [];
    private Product() { }
    public Product(Guid farmerId, Guid categoryId, string name, decimal price, ProductUnit unit, decimal stock)
    { 
        FarmerId = farmerId; 
        CategoryId = categoryId; 
        Name = name.Trim(); 
        SetPrice(price); 
        Unit = unit; 
        SetStock(stock); 
    }
    public Guid FarmerId { get; private set; }
    public Guid CategoryId { get; private set; }
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public decimal Price { get; private set; }
    public ProductUnit Unit { get; private set; }
    public decimal Stock { get; private set; }
    public ProductStatus Status { get; private set; } = ProductStatus.Draft;
    public bool IsAvailable { get; private set; } = true;
    public Farmer Farmer { get; private set; } = null!;
    public Category Category { get; private set; } = null!;
    public IReadOnlyCollection<ProductImage> Images => _images;
    public void SetPrice(decimal price)
    {
        if (price < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(price));
        }

        Price = price; MarkUpdated();
    }
    public void SetStock(decimal stock) 
    {
        if (stock < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(stock));
        }

        Stock = stock; MarkUpdated();
    }
    public void SetStatus(ProductStatus status)
    {
        Status = status; 
        MarkUpdated();
    }
}
