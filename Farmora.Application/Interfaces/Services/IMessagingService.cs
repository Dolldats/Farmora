using Farmora.Application.DTO.Messaging;

namespace Farmora.Application.Interfaces.Services;

public interface IMessagingService
{
    Task<ConversationResponse> CreateConversationAsync(Guid creatorId, CreateConversationDto dto, CancellationToken cancellationToken);
    Task<MessageResponse> SendAsync(Guid senderId, SendMessageDto dto, CancellationToken cancellationToken);
    Task<IReadOnlyList<ConversationResponse>> ListMineAsync(Guid userId, CancellationToken cancellationToken);
    Task<IReadOnlyList<MessageResponse>> ReadConversationAsync(Guid userId, Guid conversationId, int page, int pageSize, CancellationToken cancellationToken);
    Task<bool> MarkMessageReadAsync(Guid userId, Guid messageId, CancellationToken cancellationToken);
}
