using Farmora.Domain.Entities;

namespace Farmora.Application.Interfaces.Repositories;

public interface IConversationRepository
{
    Task<Conversation?> GetAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<Conversation>> GetByUserAsync(Guid userId, CancellationToken cancellationToken);
    Task AddAsync(Conversation conversation, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}

public interface IMessageRepository
{
    Task<Message?> GetAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<Message>> GetByConversationPagedAsync(Guid conversationId, int page, int pageSize, CancellationToken cancellationToken);
    Task<int> CountUnreadAsync(Guid conversationId, Guid userId, CancellationToken cancellationToken);
    Task AddAsync(Message message, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
