using Farmora.Domain.Entities;

namespace Farmora.Application.Interfaces.Repositories;

public interface INotificationRepository
{
    Task<Notification?> GetAsync(Guid id, CancellationToken cancellationToken);
    Task<(IReadOnlyList<Notification> Items, int Total)> ListPagedAsync(Guid recipientId, bool unreadOnly, int page, int pageSize, CancellationToken cancellationToken);
    Task<int> CountUnreadAsync(Guid recipientId, CancellationToken cancellationToken);
    Task AddAsync(Notification notification, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
