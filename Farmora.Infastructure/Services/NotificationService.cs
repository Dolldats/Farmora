using Farmora.Application.DTO.Common;
using Farmora.Application.DTO.Notifications;
using Farmora.Application.Interfaces.Services;
using Farmora.Domain.Entities;
using Farmora.Infastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Farmora.Infastructure.Services;

public sealed class NotificationService(FarmoraDbContext db) : INotificationService
{
    public async Task<NotificationResponse> CreateAsync(CreateNotificationDto dto, CancellationToken cancellationToken)
    {
        if (dto.RecipientId == Guid.Empty)
            throw new ArgumentException("Recipient id is required.");
        if (string.IsNullOrWhiteSpace(dto.Title))
            throw new ArgumentException("Title is required.");
        if (string.IsNullOrWhiteSpace(dto.Body))
            throw new ArgumentException("Body is required.");

        var notification = new Notification(dto.RecipientId, dto.Title.Trim(), dto.Body.Trim(), dto.Channel);
        db.Set<Notification>().Add(notification);
        await db.SaveChangesAsync(cancellationToken);
        return Map(notification);
    }

    public async Task<PagedResult<NotificationResponse>> ListAsync(
        Guid recipientId, int page, int pageSize, bool unreadOnly, CancellationToken cancellationToken)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var query = db.Set<Notification>().AsNoTracking().Where(n => n.RecipientId == recipientId);
        if (unreadOnly)
            query = query.Where(n => !n.IsRead);
        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(n => n.CreatedAtUtc)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(n => new NotificationResponse
            {
                Id = n.Id,
                RecipientId = n.RecipientId,
                Title = n.Title,
                Body = n.Body,
                Channel = n.Channel.ToString(),
                IsRead = n.IsRead,
                ReadAtUtc = n.ReadAtUtc,
                CreatedAtUtc = n.CreatedAtUtc,
            })
            .ToListAsync(cancellationToken);
        return new PagedResult<NotificationResponse> { Items = items, Page = page, PageSize = pageSize, TotalCount = total };
    }

    public async Task<bool> MarkReadAsync(Guid notificationId, Guid recipientId, CancellationToken cancellationToken)
    {
        var notification = await db.Set<Notification>()
            .SingleOrDefaultAsync(n => n.Id == notificationId, cancellationToken);
        if (notification is null || notification.RecipientId != recipientId)
            return false;
        if (notification.IsRead)
            return true;
        notification.MarkRead();
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<int> MarkAllReadAsync(Guid recipientId, CancellationToken cancellationToken)
    {
        var unread = await db.Set<Notification>()
            .Where(n => n.RecipientId == recipientId && !n.IsRead)
            .ToListAsync(cancellationToken);
        foreach (var n in unread)
            n.MarkRead();
        await db.SaveChangesAsync(cancellationToken);
        return unread.Count;
    }

    private static NotificationResponse Map(Notification n) => new()
    {
        Id = n.Id,
        RecipientId = n.RecipientId,
        Title = n.Title,
        Body = n.Body,
        Channel = n.Channel.ToString(),
        IsRead = n.IsRead,
        ReadAtUtc = n.ReadAtUtc,
        CreatedAtUtc = n.CreatedAtUtc,
    };
}
