using Farmora.Domain.Common;
using Farmora.Domain.Enums;

namespace Farmora.Domain.Entities;

public sealed class Review : AuditableEntity
{
    private Review() { }
    public Review(Guid orderItemId, Guid customerId, Guid productId, int rating, string? comment)
    {
        OrderItemId = orderItemId;
        CustomerId = customerId;
        ProductId = productId;
        Rating = rating;
        Comment = comment?.Trim();
        Status = ReviewStatus.Pending;
    }

    public Guid OrderItemId { get; private set; }
    public Guid CustomerId { get; private set; }
    public Guid ProductId { get; private set; }
    public int Rating { get; private set; }
    public string? Comment { get; private set; }
    public ReviewStatus Status { get; private set; }

    public void Moderate(ReviewStatus status) { Status = status; MarkUpdated(); }
}

public sealed class Report : AuditableEntity
{
    private Report() { }
    public Report(Guid reporterId, string targetType, Guid targetId, string reason)
    {
        ReporterId = reporterId;
        TargetType = targetType;
        TargetId = targetId;
        Reason = reason.Trim();
        Status = ReportStatus.Open;
    }

    public Guid ReporterId { get; private set; }
    public string TargetType { get; private set; } = null!;
    public Guid TargetId { get; private set; }
    public string Reason { get; private set; } = null!;
    public string? EvidenceUrl { get; private set; }
    public ReportStatus Status { get; private set; }
    public Guid? AssignedTo { get; private set; }
    public string? Resolution { get; private set; }

    public void Assign(Guid adminId) { AssignedTo = adminId; Status = ReportStatus.InReview; MarkUpdated(); }
    public void Resolve(string resolution, bool dismissed = false)
    {
        Resolution = resolution;
        Status = dismissed ? ReportStatus.Dismissed : ReportStatus.Resolved;
        MarkUpdated();
    }
    public void SetEvidence(string url) { EvidenceUrl = url; MarkUpdated(); }
}

public sealed class Notification : AuditableEntity
{
    private Notification() { }
    public Notification(Guid recipientId, string title, string body, NotificationChannel channel = NotificationChannel.InApp)
    {
        RecipientId = recipientId;
        Title = title;
        Body = body;
        Channel = channel;
    }

    public Guid RecipientId { get; private set; }
    public string Title { get; private set; } = null!;
    public string Body { get; private set; } = null!;
    public NotificationChannel Channel { get; private set; }
    public bool IsRead { get; private set; }
    public DateTime? ReadAtUtc { get; private set; }

    public void MarkRead() { IsRead = true; ReadAtUtc = DateTime.UtcNow; MarkUpdated(); }
}

public sealed class AuditLog : Entity
{
    private AuditLog() { }
    public AuditLog(Guid? actorId, string action, string resource, Guid? resourceId, string? detail = null)
    {
        ActorId = actorId;
        Action = action;
        Resource = resource;
        ResourceId = resourceId;
        Detail = detail;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public Guid? ActorId { get; private set; }
    public string Action { get; private set; } = null!;
    public string Resource { get; private set; } = null!;
    public Guid? ResourceId { get; private set; }
    public string? Detail { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
}
