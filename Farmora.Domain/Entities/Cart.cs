using Farmora.Domain.Common;

namespace Farmora.Domain.Entities;

public sealed class Cart : AuditableEntity
{
    private readonly List<CartItem> _items = [];
    private Cart() { }
    public Cart(Guid customerId) => CustomerId = customerId;

    public Guid CustomerId { get; private set; }
    public bool IsActive { get; private set; } = true;
    public Customer Customer { get; private set; } = null!;
    public IReadOnlyCollection<CartItem> Items => _items;

    public void Deactivate() { IsActive = false; MarkUpdated(); }
}

public sealed class CartItem : Entity
{
    private CartItem() { }
    public CartItem(Guid cartId, Guid productId, decimal quantity)
    {
        CartId = cartId;
        ProductId = productId;
        Quantity = quantity;
    }

    public Guid CartId { get; private set; }
    public Guid ProductId { get; private set; }
    public decimal Quantity { get; private set; }
    public Cart Cart { get; private set; } = null!;
    public Product Product { get; private set; } = null!;

    public void SetQuantity(decimal qty)
    {
        if (qty <= 0) throw new ArgumentOutOfRangeException(nameof(qty));
        Quantity = qty;
    }
}

public sealed class WishlistItem : AuditableEntity
{
    private WishlistItem() { }
    public WishlistItem(Guid customerId, Guid productId)
    {
        CustomerId = customerId;
        ProductId = productId;
    }

    public Guid CustomerId { get; private set; }
    public Guid ProductId { get; private set; }
    public Customer Customer { get; private set; } = null!;
    public Product Product { get; private set; } = null!;
}
