using Farmora.Domain.Common;
using Farmora.Domain.Enums;

namespace Farmora.Domain.Entities;

public sealed class Promotion : AuditableEntity
{
    private readonly List<CouponRedemption> _redemptions = [];
    private Promotion() { }
    public Promotion(string code, PromotionType type, decimal value, DateTime startsAtUtc, DateTime endsAtUtc, int? maxUses = null)
    {
        Code = code.Trim().ToUpperInvariant();
        Type = type;
        Value = value;
        StartsAtUtc = startsAtUtc;
        EndsAtUtc = endsAtUtc;
        MaxUses = maxUses;
        Status = CouponStatus.Active;
    }

    public string Code { get; private set; } = null!;
    public PromotionType Type { get; private set; }
    public decimal Value { get; private set; }
    public DateTime StartsAtUtc { get; private set; }
    public DateTime EndsAtUtc { get; private set; }
    public int? MaxUses { get; private set; }
    public int UseCount { get; private set; }
    public CouponStatus Status { get; private set; }
    public decimal? MinOrderAmount { get; private set; }
    public IReadOnlyCollection<CouponRedemption> Redemptions => _redemptions;

    public bool IsUsable(DateTime now) =>
        Status == CouponStatus.Active && now >= StartsAtUtc && now <= EndsAtUtc && (MaxUses is null || UseCount < MaxUses);

    public void SetMinOrder(decimal? min) { MinOrderAmount = min; MarkUpdated(); }
    public void Disable() { Status = CouponStatus.Disabled; MarkUpdated(); }
    public void RecordRedemption(CouponRedemption r)
    {
        _redemptions.Add(r);
        UseCount++;
        if (MaxUses is not null && UseCount >= MaxUses) Status = CouponStatus.Exhausted;
        MarkUpdated();
    }

    public decimal DiscountFor(decimal subtotal) =>
        Type == PromotionType.Percentage ? Math.Round(subtotal * Value / 100m, 2) : Math.Min(Value, subtotal);
}

public sealed class CouponRedemption : Entity
{
    private CouponRedemption() { }
    public CouponRedemption(Guid promotionId, Guid orderId, Guid customerId, decimal discount)
    {
        PromotionId = promotionId;
        OrderId = orderId;
        CustomerId = customerId;
        Discount = discount;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public Guid PromotionId { get; private set; }
    public Guid OrderId { get; private set; }
    public Guid CustomerId { get; private set; }
    public decimal Discount { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
}

public sealed class AnalyticsEvent : Entity
{
    private AnalyticsEvent() { }
    public AnalyticsEvent(string eventType, Guid? actorId, Guid? resourceId, string? metadata = null)
    {
        EventType = eventType;
        ActorId = actorId;
        ResourceId = resourceId;
        Metadata = metadata;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public string EventType { get; private set; } = null!;
    public Guid? ActorId { get; private set; }
    public Guid? ResourceId { get; private set; }
    public string? Metadata { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
}
