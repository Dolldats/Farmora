using Farmora.Domain.Enums;

namespace Farmora.Application.DTO.Notifications;

public sealed class CreateNotificationDto
{
    public Guid RecipientId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public NotificationChannel Channel { get; set; } = NotificationChannel.InApp;
}

public sealed class NotificationResponse
{
    public Guid Id { get; init; }
    public Guid RecipientId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
    public string Channel { get; init; } = string.Empty;
    public bool IsRead { get; init; }
    public DateTime? ReadAtUtc { get; init; }
    public DateTime CreatedAtUtc { get; init; }
}
