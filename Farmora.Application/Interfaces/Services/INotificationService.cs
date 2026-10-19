using Farmora.Application.DTO.Common;
using Farmora.Application.DTO.Notifications;

namespace Farmora.Application.Interfaces.Services;

public interface INotificationService
{
    Task<NotificationResponse> CreateAsync(CreateNotificationDto dto, CancellationToken cancellationToken);
    Task<PagedResult<NotificationResponse>> ListAsync(Guid recipientId, int page, int pageSize, bool unreadOnly, CancellationToken cancellationToken);
    Task<bool> MarkReadAsync(Guid notificationId, Guid recipientId, CancellationToken cancellationToken);
    Task<int> MarkAllReadAsync(Guid recipientId, CancellationToken cancellationToken);
}
