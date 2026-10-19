using Farmora.Application.Interfaces.Repositories;
using Farmora.Domain.Entities;
using Farmora.Infastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Farmora.Infastructure.Repositories;

public sealed class NotificationRepository(FarmoraDbContext db) : INotificationRepository
{
    public Task<Notification?> GetAsync(Guid id, CancellationToken cancellationToken) =>
        db.Set<Notification>().SingleOrDefaultAsync(n => n.Id == id, cancellationToken);

    public async Task<(IReadOnlyList<Notification> Items, int Total)> ListPagedAsync(
        Guid recipientId, bool unreadOnly, int page, int pageSize, CancellationToken cancellationToken)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var query = db.Set<Notification>().AsNoTracking().Where(n => n.RecipientId == recipientId);
        if (unreadOnly)
            query = query.Where(n => !n.IsRead);
        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(n => n.CreatedAtUtc)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return (items, total);
    }

    public Task<int> CountUnreadAsync(Guid recipientId, CancellationToken cancellationToken) =>
        db.Set<Notification>().CountAsync(n => n.RecipientId == recipientId && !n.IsRead, cancellationToken);

    public Task AddAsync(Notification notification, CancellationToken cancellationToken) =>
        db.Set<Notification>().AddAsync(notification, cancellationToken).AsTask();

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        db.SaveChangesAsync(cancellationToken);
}
