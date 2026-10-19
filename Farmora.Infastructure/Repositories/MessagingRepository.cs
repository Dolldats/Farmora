using Farmora.Application.Interfaces.Repositories;
using Farmora.Domain.Entities;
using Farmora.Infastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Farmora.Infastructure.Repositories;

public sealed class MessagingRepository(FarmoraDbContext db) : IConversationRepository, IMessageRepository
{
    public Task<Conversation?> GetAsync(Guid id, CancellationToken cancellationToken) =>
        db.Set<Conversation>().SingleOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Conversation>> GetByUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        var conversationIds = await db.Set<ConversationParticipant>().AsNoTracking()
            .Where(p => p.UserId == userId)
            .Select(p => p.ConversationId)
            .ToListAsync(cancellationToken);
        if (conversationIds.Count == 0)
            return [];
        return await db.Set<Conversation>().AsNoTracking()
            .Where(c => conversationIds.Contains(c.Id))
            .OrderByDescending(c => c.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    Task IConversationRepository.AddAsync(Conversation conversation, CancellationToken cancellationToken) =>
        db.Set<Conversation>().AddAsync(conversation, cancellationToken).AsTask();

    async Task<Message?> IMessageRepository.GetAsync(Guid id, CancellationToken cancellationToken) =>
        await db.Set<Message>().SingleOrDefaultAsync(m => m.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Message>> GetByConversationPagedAsync(
        Guid conversationId, int page, int pageSize, CancellationToken cancellationToken)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        return await db.Set<Message>().AsNoTracking()
            .Where(m => m.ConversationId == conversationId)
            .OrderBy(m => m.CreatedAtUtc)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountUnreadAsync(Guid conversationId, Guid userId, CancellationToken cancellationToken) =>
        db.Set<Message>().CountAsync(
            m => m.ConversationId == conversationId && !m.IsRead && m.SenderId != userId,
            cancellationToken);

    Task IMessageRepository.AddAsync(Message message, CancellationToken cancellationToken) =>
        db.Set<Message>().AddAsync(message, cancellationToken).AsTask();

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        db.SaveChangesAsync(cancellationToken);
}
