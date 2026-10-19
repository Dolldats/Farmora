using Farmora.Domain.Common;
using Farmora.Domain.Enums;

namespace Farmora.Domain.Entities;

public sealed class Payment : AuditableEntity
{
    private readonly List<PaymentEvent> _events = [];
    private Payment() { }
    public Payment(Guid orderId, decimal amount, string currency = "NGN")
    {
        OrderId = orderId;
        Amount = amount;
        Currency = currency;
        Status = PaymentStatus.Pending;
        ProviderReference = Guid.NewGuid().ToString("N");
    }

    public Guid OrderId { get; private set; }
    public string Provider { get; private set; } = "Stub";
    public string ProviderReference { get; private set; } = null!;
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "NGN";
    public PaymentStatus Status { get; private set; }
    public Order Order { get; private set; } = null!;
    public IReadOnlyCollection<PaymentEvent> Events => _events;

    public void SetStatus(PaymentStatus status) { Status = status; MarkUpdated(); }
    public void AddEvent(PaymentEvent evt) => _events.Add(evt);
}

public sealed class PaymentEvent : Entity
{
    private PaymentEvent() { }
    public PaymentEvent(Guid paymentId, string eventId, string eventType, string payload)
    {
        PaymentId = paymentId;
        EventId = eventId;
        EventType = eventType;
        Payload = payload;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public Guid PaymentId { get; private set; }
    public string EventId { get; private set; } = null!;
    public string EventType { get; private set; } = null!;
    public string Payload { get; private set; } = null!;
    public DateTime CreatedAtUtc { get; private set; }
}
