using Farmora.Application.Common;
using Farmora.Application.DTO.Orders;
using Farmora.Application.Interfaces.Repositories;
using Farmora.Application.Interfaces.Services;
using Farmora.Domain.Entities;
using Farmora.Domain.Enums;
using Farmora.Infastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Farmora.Infastructure.Services;

public sealed class OrderService(
    IOrderRepository orders,
    ICartRepository carts,
    IPromotionRepository promotions,
    FarmoraDbContext db) : IOrderService
{
    public async Task<QuoteResponse> QuoteAsync(Guid customerId, QuoteRequest request, CancellationToken cancellationToken)
    {
        var customer = await RequireCustomerAsync(customerId, cancellationToken);
        var cart = await carts.GetActiveWithItemsAsync(customer.Id, cancellationToken)
            ?? throw new InvalidOperationException("Active cart not found.");
        if (cart.Items.Count == 0)
            throw new InvalidOperationException("Cart is empty.");

        var priced = await PriceCartLinesAsync(cart, cancellationToken);
        var subtotal = priced.Sum(l => l.LineTotal);
        var discount = await DiscountForAsync(request.PromoCode, subtotal, cancellationToken);
        var fee = FeeFor(request.DeliveryMethod);

        return new QuoteResponse
        {
            Subtotal = subtotal,
            Discount = discount,
            DeliveryFee = fee,
            Total = subtotal - discount + fee,
            Currency = "NGN",
        };
    }

    public async Task<OrderResponse> CreateAsync(Guid customerId, CreateOrderDto dto, CancellationToken cancellationToken)
    {
        var customer = await RequireCustomerAsync(customerId, cancellationToken);

        if (!string.IsNullOrWhiteSpace(dto.IdempotencyKey))
        {
            var seen = await orders.GetByIdempotencyKeyAsync(customer.Id, dto.IdempotencyKey, cancellationToken);
            if (seen is not null)
                return await MapAsync(seen, cancellationToken);
        }

        await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);

        var cart = await carts.GetActiveWithItemsAsync(customer.Id, cancellationToken)
            ?? throw new InvalidOperationException("Active cart not found.");
        if (cart.Items.Count == 0)
            throw new InvalidOperationException("Cart is empty.");

        var priced = await PriceCartLinesAsync(cart, cancellationToken);
        var subtotal = priced.Sum(l => l.LineTotal);

        Promotion? promo = null;
        decimal discount = 0;
        if (!string.IsNullOrWhiteSpace(dto.PromoCode))
        {
            promo = await promotions.GetByCodeAsync(dto.PromoCode, cancellationToken)
                ?? throw new InvalidOperationException("Promotion code not found.");
            discount = ValidatePromo(promo, subtotal);
        }

        var fee = FeeFor(dto.DeliveryMethod);
        var snapshot = await SnapshotForAsync(customer.Id, dto.AddressId, cancellationToken);

        var order = new Order(customer.Id, snapshot, subtotal, discount, fee);
        if (!string.IsNullOrWhiteSpace(dto.IdempotencyKey))
            order.SetIdempotencyKey(dto.IdempotencyKey);

        foreach (var line in priced)
        {
            line.Product.SetStock(line.Product.Stock - line.Quantity);
            order.AddItem(new OrderItem(
                order.Id,
                line.Product.Id,
                line.Product.FarmerId,
                line.Product.Name,
                line.Product.Price,
                line.Product.Unit.ToString(),
                line.Quantity));
        }

        order.TransitionTo(OrderStatus.PaymentPending, "Order created");
        await orders.AddAsync(order, cancellationToken);

        if (promo is not null && discount > 0)
            promo.RecordRedemption(new CouponRedemption(promo.Id, order.Id, customer.Id, discount));

        await db.Set<Payment>().AddAsync(new Payment(order.Id, order.Total, order.Currency), cancellationToken);
        await db.Set<Delivery>().AddAsync(new Delivery(order.Id, dto.DeliveryMethod, fee), cancellationToken);

        cart.Deactivate();
        db.Set<CartItem>().RemoveRange(cart.Items);

        await db.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);

        var created = (await orders.GetByIdWithDetailsAsync(order.Id, cancellationToken))!;
        return await MapAsync(created, cancellationToken);
    }

    public async Task<PagedResult<OrderResponse>> GetMyOrdersAsync(Guid customerId, int page, int pageSize, CancellationToken cancellationToken)
    {
        var customer = await RequireCustomerAsync(customerId, cancellationToken);
        var (items, total) = await orders.ListByCustomerPagedAsync(customer.Id, page, pageSize, cancellationToken);
        var mapped = new List<OrderResponse>();
        foreach (var o in items)
            mapped.Add(await MapAsync(o, cancellationToken));
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 20 : pageSize;
        return PagedResult<OrderResponse>.Create(mapped, total, page, pageSize);
    }

    public async Task<OrderResponse?> GetByIdAsync(Guid customerId, Guid orderId, CancellationToken cancellationToken)
    {
        var customer = await RequireCustomerAsync(customerId, cancellationToken);
        var order = await orders.GetByIdWithDetailsAsync(orderId, cancellationToken);
        if (order is null || order.CustomerId != customer.Id) return null;
        return await MapAsync(order, cancellationToken);
    }

    public async Task<bool> CancelAsync(Guid customerId, Guid orderId, CancellationToken cancellationToken)
    {
        var customer = await RequireCustomerAsync(customerId, cancellationToken);
        var order = await orders.GetByIdWithDetailsAsync(orderId, cancellationToken);
        if (order is null || order.CustomerId != customer.Id) return false;
        if (order.Status is not (OrderStatus.Pending or OrderStatus.PaymentPending)) return false;
        order.TransitionTo(OrderStatus.Cancelled, "Cancelled by customer");
        await orders.SaveChangesAsync(cancellationToken);
        return true;
    }

    private sealed record PricedLine(Product Product, decimal Quantity, decimal LineTotal);

    private async Task<List<PricedLine>> PriceCartLinesAsync(Cart cart, CancellationToken ct)
    {
        var productIds = cart.Items.Select(i => i.ProductId).ToList();
        var products = await db.Set<Product>()
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, ct);

        var lines = new List<PricedLine>();
        foreach (var item in cart.Items)
        {
            if (!products.TryGetValue(item.ProductId, out var product))
                throw new InvalidOperationException("Product not found.");
            if (!product.IsAvailable)
                throw new InvalidOperationException($"Product '{product.Name}' is not available.");
            if (product.Stock <= 0 || item.Quantity > product.Stock)
                throw new InvalidOperationException($"Insufficient stock for '{product.Name}'.");
            lines.Add(new PricedLine(product, item.Quantity, product.Price * item.Quantity));
        }
        return lines;
    }

    private async Task<decimal> DiscountForAsync(string? promoCode, decimal subtotal, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(promoCode)) return 0;
        var promo = await promotions.GetByCodeAsync(promoCode, ct)
            ?? throw new InvalidOperationException("Promotion code not found.");
        return ValidatePromo(promo, subtotal);
    }

    private static decimal ValidatePromo(Promotion promo, decimal subtotal)
    {
        var now = DateTime.UtcNow;
        if (!promo.IsUsable(now))
            throw new InvalidOperationException("Promotion is not usable.");
        if (promo.MinOrderAmount is not null && subtotal < promo.MinOrderAmount)
            throw new InvalidOperationException("Order subtotal does not meet the promotion minimum.");
        return promo.DiscountFor(subtotal);
    }

    private async Task<string> SnapshotForAsync(Guid customerId, Guid? addressId, CancellationToken ct)
    {
        if (addressId.HasValue)
        {
            var owned = await db.Set<Address>()
                .FirstOrDefaultAsync(a => a.Id == addressId.Value && a.CustomerId == customerId, ct)
                ?? throw new InvalidOperationException("Address not found.");
            return owned.Snapshot();
        }
        var fallback = await db.Set<Address>()
            .Where(a => a.CustomerId == customerId)
            .OrderByDescending(a => a.IsDefault)
            .ThenBy(a => a.CreatedAtUtc)
            .FirstOrDefaultAsync(ct);
        return fallback?.Snapshot() ?? string.Empty;
    }

    private static decimal FeeFor(DeliveryMethod method) => method switch
    {
        DeliveryMethod.Pickup => 0m,
        DeliveryMethod.FarmerDelivery => 500m,
        _ => 1000m,
    };

    private async Task<Customer> RequireCustomerAsync(Guid idOrUserId, CancellationToken ct) =>
        await db.Set<Customer>().FirstOrDefaultAsync(c => c.Id == idOrUserId, ct)
        ?? await db.Set<Customer>().FirstOrDefaultAsync(c => c.UserId == idOrUserId, ct)
        ?? throw new InvalidOperationException("Customer profile required.");

    private async Task<OrderResponse> MapAsync(Order o, CancellationToken ct)
    {
        var deliveryStatus = await db.Set<Delivery>()
            .AsNoTracking()
            .Where(d => d.OrderId == o.Id)
            .Select(d => (DeliveryStatus?)d.Status)
            .FirstOrDefaultAsync(ct);
        return new OrderResponse
        {
            Id = o.Id,
            CustomerId = o.CustomerId,
            OrderNumber = o.OrderNumber,
            AddressSnapshot = o.AddressSnapshot,
            Subtotal = o.Subtotal,
            DiscountTotal = o.DiscountTotal,
            DeliveryFee = o.DeliveryFee,
            Total = o.Total,
            Currency = o.Currency,
            Status = o.Status,
            DeliveryStatus = deliveryStatus,
            IdempotencyKey = o.IdempotencyKey,
            CreatedAtUtc = o.CreatedAtUtc,
            Items = o.Items.Select(i => new OrderItemResponse
            {
                Id = i.Id,
                ProductId = i.ProductId,
                FarmerId = i.FarmerId,
                ProductNameSnapshot = i.ProductNameSnapshot,
                UnitPriceSnapshot = i.UnitPriceSnapshot,
                UnitSnapshot = i.UnitSnapshot,
                Quantity = i.Quantity,
                LineTotal = i.LineTotal,
            }).ToList(),
        };
    }
}
