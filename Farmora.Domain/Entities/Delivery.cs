using Farmora.Domain.Common;
using Farmora.Domain.Enums;

namespace Farmora.Domain.Entities;

public sealed class Delivery : AuditableEntity
{
    private readonly List<DeliveryStatusHistory> _timeline = [];
    private Delivery() { }
    public Delivery(Guid orderId, DeliveryMethod method, decimal fee)
    {
        OrderId = orderId;
        Method = method;
        Fee = fee;
        Status = DeliveryStatus.Pending;
    }

    public Guid OrderId { get; private set; }
    public DeliveryMethod Method { get; private set; }
    public decimal Fee { get; private set; }
    public string? TrackingNumber { get; private set; }
    public DeliveryStatus Status { get; private set; }
    public Order Order { get; private set; } = null!;
    public IReadOnlyCollection<DeliveryStatusHistory> Timeline => _timeline;

    public void UpdateStatus(DeliveryStatus next, string? note = null)
    {
        Status = next;
        _timeline.Add(new DeliveryStatusHistory(Id, next, note));
        MarkUpdated();
    }

    public void SetTracking(string tracking) { TrackingNumber = tracking; MarkUpdated(); }
}

public sealed class DeliveryStatusHistory : Entity
{
    private DeliveryStatusHistory() { }
    public DeliveryStatusHistory(Guid deliveryId, DeliveryStatus status, string? note)
    {
        DeliveryId = deliveryId;
        Status = status;
        Note = note;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public Guid DeliveryId { get; private set; }
    public DeliveryStatus Status { get; private set; }
    public string? Note { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
}
