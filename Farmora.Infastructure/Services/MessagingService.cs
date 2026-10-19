using System.Collections.Concurrent;
using Farmora.Application.DTO.Messaging;
using Farmora.Application.Interfaces.Services;
using Farmora.Domain.Entities;
using Farmora.Infastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Farmora.Infastructure.Services;

public sealed class MessagingService(FarmoraDbContext db) : IMessagingService
{
    private const int MaxMessagesPerMinute = 20;
    private static readonly TimeSpan RateWindow = TimeSpan.FromMinutes(1);
    private static readonly ConcurrentDictionary<Guid, List<DateTime>> SendHistory = new();
    private static readonly ConcurrentDictionary<Guid, object> ConversationLocks = new();

    public async Task<ConversationResponse> CreateConversationAsync(Guid creatorId, CreateConversationDto dto, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dto.Subject))
            throw new ArgumentException("Subject is required.");
        if (string.IsNullOrWhiteSpace(dto.FirstMessage))
            throw new ArgumentException("First message is required.");
        var participantIds = dto.ParticipantIds.Distinct().ToList();
        if (!participantIds.Contains(creatorId))
            throw new ArgumentException("Participants must include the sender.");
        if (participantIds.Count < 2)
            throw new ArgumentException("A conversation requires at least two participants.");

        var conversation = new Conversation(dto.Subject);
        db.Set<Conversation>().Add(conversation);
        await db.SaveChangesAsync(cancellationToken);

        var participants = participantIds.Select(id => new ConversationParticipant(conversation.Id, id)).ToList();
        foreach (var p in participants)
            conversation.AddParticipant(p);
        var first = new Message(conversation.Id, creatorId, dto.FirstMessage);
        conversation.AddMessage(first);
        db.Set<ConversationParticipant>().AddRange(participants);
        db.Set<Message>().Add(first);
        await db.SaveChangesAsync(cancellationToken);

        return new ConversationResponse
        {
            Id = conversation.Id,
            Subject = conversation.Subject,
            ParticipantIds = participantIds,
            LastMessage = Map(first),
            UnreadCount = 0,
            CreatedAtUtc = conversation.CreatedAtUtc,
        };
    }

    public async Task<MessageResponse> SendAsync(Guid senderId, SendMessageDto dto, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dto.Body))
            throw new ArgumentException("Message body is required.");

        var participants = await db.Set<ConversationParticipant>().AsNoTracking()
            .Where(p => p.ConversationId == dto.ConversationId)
            .ToListAsync(cancellationToken);
        var sender = participants.SingleOrDefault(p => p.UserId == senderId)
            ?? throw new UnauthorizedAccessException("Sender is not a participant of this conversation.");
        if (sender.IsBlocked)
            throw new InvalidOperationException("Sender is blocked in this conversation.");
        if (participants.Any(p => p.IsBlocked && p.UserId != senderId))
            throw new InvalidOperationException("Conversation is blocked.");

        CheckRateLimit(dto.ConversationId);

        var message = new Message(dto.ConversationId, senderId, dto.Body);
        db.Set<Message>().Add(message);
        await db.SaveChangesAsync(cancellationToken);
        return Map(message);
    }

    public async Task<IReadOnlyList<ConversationResponse>> ListMineAsync(Guid userId, CancellationToken cancellationToken)
    {
        var conversationIds = await db.Set<ConversationParticipant>().AsNoTracking()
            .Where(p => p.UserId == userId)
            .Select(p => p.ConversationId)
            .Distinct()
            .ToListAsync(cancellationToken);
        if (conversationIds.Count == 0)
            return [];

        var conversations = await db.Set<Conversation>().AsNoTracking()
            .Where(c => conversationIds.Contains(c.Id))
            .OrderByDescending(c => c.CreatedAtUtc)
            .ToListAsync(cancellationToken);
        var participants = await db.Set<ConversationParticipant>().AsNoTracking()
            .Where(p => conversationIds.Contains(p.ConversationId))
            .ToListAsync(cancellationToken);
        var messages = await db.Set<Message>().AsNoTracking()
            .Where(m => conversationIds.Contains(m.ConversationId))
            .ToListAsync(cancellationToken);

        return conversations.Select(c =>
        {
            var convoMessages = messages.Where(m => m.ConversationId == c.Id).OrderBy(m => m.CreatedAtUtc).ToList();
            return new ConversationResponse
            {
                Id = c.Id,
                Subject = c.Subject,
                ParticipantIds = participants.Where(p => p.ConversationId == c.Id).Select(p => p.UserId).ToList(),
                LastMessage = convoMessages.Count == 0 ? null : Map(convoMessages[^1]),
                UnreadCount = convoMessages.Count(m => !m.IsRead && m.SenderId != userId),
                CreatedAtUtc = c.CreatedAtUtc,
            };
        }).ToList();
    }

    public async Task<IReadOnlyList<MessageResponse>> ReadConversationAsync(
        Guid userId, Guid conversationId, int page, int pageSize, CancellationToken cancellationToken)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var isParticipant = await db.Set<ConversationParticipant>().AsNoTracking()
            .AnyAsync(p => p.ConversationId == conversationId && p.UserId == userId, cancellationToken);
        if (!isParticipant)
            throw new UnauthorizedAccessException("User is not a participant of this conversation.");

        return await db.Set<Message>().AsNoTracking()
            .Where(m => m.ConversationId == conversationId)
            .OrderBy(m => m.CreatedAtUtc)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(m => new MessageResponse
            {
                Id = m.Id,
                ConversationId = m.ConversationId,
                SenderId = m.SenderId,
                Body = m.Body,
                IsRead = m.IsRead,
                CreatedAtUtc = m.CreatedAtUtc,
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> MarkMessageReadAsync(Guid userId, Guid messageId, CancellationToken cancellationToken)
    {
        var message = await db.Set<Message>().SingleOrDefaultAsync(m => m.Id == messageId, cancellationToken);
        if (message is null)
            return false;
        var isParticipant = await db.Set<ConversationParticipant>().AsNoTracking()
            .AnyAsync(p => p.ConversationId == message.ConversationId && p.UserId == userId, cancellationToken);
        if (!isParticipant)
            return false;
        if (!message.IsRead)
        {
            message.MarkRead();
            await db.SaveChangesAsync(cancellationToken);
        }
        return true;
    }

    private static void CheckRateLimit(Guid conversationId)
    {
        var now = DateTime.UtcNow;
        var history = SendHistory.GetOrAdd(conversationId, _ => []);
        var gate = ConversationLocks.GetOrAdd(conversationId, _ => new object());
        lock (gate)
        {
            history.RemoveAll(t => now - t > RateWindow);
            if (history.Count >= MaxMessagesPerMinute)
                throw new InvalidOperationException("Rate limit exceeded: maximum 20 messages per minute per conversation.");
            history.Add(now);
        }
    }

    private static MessageResponse Map(Message m) => new()
    {
        Id = m.Id,
        ConversationId = m.ConversationId,
        SenderId = m.SenderId,
        Body = m.Body,
        IsRead = m.IsRead,
        CreatedAtUtc = m.CreatedAtUtc,
    };
}
