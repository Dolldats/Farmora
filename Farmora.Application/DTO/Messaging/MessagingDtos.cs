namespace Farmora.Application.DTO.Messaging;

public sealed class CreateConversationDto
{
    public List<Guid> ParticipantIds { get; set; } = [];
    public string Subject { get; set; } = string.Empty;
    public string FirstMessage { get; set; } = string.Empty;
}

public sealed class SendMessageDto
{
    public Guid ConversationId { get; set; }
    public string Body { get; set; } = string.Empty;
}

public sealed class MessageResponse
{
    public Guid Id { get; init; }
    public Guid ConversationId { get; init; }
    public Guid SenderId { get; init; }
    public string Body { get; init; } = string.Empty;
    public bool IsRead { get; init; }
    public DateTime CreatedAtUtc { get; init; }
}

public sealed class ConversationResponse
{
    public Guid Id { get; init; }
    public string Subject { get; init; } = string.Empty;
    public IReadOnlyList<Guid> ParticipantIds { get; init; } = [];
    public MessageResponse? LastMessage { get; init; }
    public int UnreadCount { get; init; }
    public DateTime CreatedAtUtc { get; init; }
}
