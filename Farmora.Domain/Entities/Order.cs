using Farmora.Domain.Common;
using Farmora.Domain.Enums;

namespace Farmora.Domain.Entities;

public sealed class Order : AuditableEntity
{
    private readonly List<OrderItem> _items = [];
    private readonly List<OrderStatusHistory> _history = [];
    private Order() { }

    public Order(Guid customerId, string addressSnapshot, decimal subtotal, decimal discount, decimal deliveryFee, string currency = "NGN")
    {
        CustomerId = customerId;
        AddressSnapshot = addressSnapshot;
        Subtotal = subtotal;
        DiscountTotal = discount;
        DeliveryFee = deliveryFee;
        Total = subtotal - discount + deliveryFee;
        Currency = currency;
        Status = OrderStatus.Pending;
        IdempotencyKey = Guid.NewGuid().ToString("N");
    }

    public Guid CustomerId { get; private set; }
    public string OrderNumber { get; private set; } = $"FN-{DateTime.UtcNow:yyyyMMdd}-{(Guid.NewGuid().ToString("N")[..6].ToUpperInvariant())}";
    public string AddressSnapshot { get; private set; } = null!;
    public decimal Subtotal { get; private set; }
    public decimal DiscountTotal { get; private set; }
    public decimal DeliveryFee { get; private set; }
    public decimal Total { get; private set; }
    public string Currency { get; private set; } = "NGN";
    public OrderStatus Status { get; private set; }
    public string IdempotencyKey { get; private set; } = null!;
    public Customer Customer { get; private set; } = null!;
    public IReadOnlyCollection<OrderItem> Items => _items;
    public IReadOnlyCollection<OrderStatusHistory> History => _history;

    public void SetIdempotencyKey(string key) => IdempotencyKey = key;

    public void AddItem(OrderItem item) => _items.Add(item);

    public void TransitionTo(OrderStatus next, string? note = null)
    {
        Status = next;
        _history.Add(new OrderStatusHistory(Id, next, note));
        MarkUpdated();
    }
}

public sealed class OrderItem : Entity
{
    private OrderItem() { }
    public OrderItem(Guid orderId, Guid productId, Guid farmerId, string productName, decimal unitPrice, string unit, decimal quantity)
    {
        OrderId = orderId;
        ProductId = productId;
        FarmerId = farmerId;
        ProductNameSnapshot = productName;
        UnitPriceSnapshot = unitPrice;
        UnitSnapshot = unit;
        Quantity = quantity;
        LineTotal = unitPrice * quantity;
    }

    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid FarmerId { get; private set; }
    public string ProductNameSnapshot { get; private set; } = null!;
    public decimal UnitPriceSnapshot { get; private set; }
    public string UnitSnapshot { get; private set; } = null!;
    public decimal Quantity { get; private set; }
    public decimal LineTotal { get; private set; }
    public Order Order { get; private set; } = null!;
}

public sealed class OrderStatusHistory : Entity
{
    private OrderStatusHistory() { }
    public OrderStatusHistory(Guid orderId, OrderStatus status, string? note)
    {
        OrderId = orderId;
        Status = status;
        Note = note;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public Guid OrderId { get; private set; }
    public OrderStatus Status { get; private set; }
    public string? Note { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
}
