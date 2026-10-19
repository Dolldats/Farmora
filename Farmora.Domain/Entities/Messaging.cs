using Farmora.Domain.Common;

namespace Farmora.Domain.Entities;

public sealed class Conversation : AuditableEntity
{
    private readonly List<Message> _messages = [];
    private readonly List<ConversationParticipant> _participants = [];
    private Conversation() { }
    public Conversation(string subject)
    {
        Subject = subject.Trim();
    }

    public string Subject { get; private set; } = null!;
    public IReadOnlyCollection<Message> Messages => _messages;
    public IReadOnlyCollection<ConversationParticipant> Participants => _participants;

    public void AddParticipant(ConversationParticipant p) => _participants.Add(p);
    public void AddMessage(Message m) => _messages.Add(m);
}

public sealed class ConversationParticipant : Entity
{
    private ConversationParticipant() { }
    public ConversationParticipant(Guid conversationId, Guid userId)
    {
        ConversationId = conversationId;
        UserId = userId;
    }

    public Guid ConversationId { get; private set; }
    public Guid UserId { get; private set; }
    public bool IsBlocked { get; private set; }
    public void Block() => IsBlocked = true;
}

public sealed class Message : AuditableEntity
{
    private Message() { }
    public Message(Guid conversationId, Guid senderId, string body)
    {
        ConversationId = conversationId;
        SenderId = senderId;
        Body = body.Trim();
    }

    public Guid ConversationId { get; private set; }
    public Guid SenderId { get; private set; }
    public string Body { get; private set; } = null!;
    public bool IsRead { get; private set; }
    public DateTime? ReadAtUtc { get; private set; }
    public void MarkRead() { IsRead = true; ReadAtUtc = DateTime.UtcNow; }
}

public sealed class FarmerFollow : AuditableEntity
{
    private FarmerFollow() { }
    public FarmerFollow(Guid customerId, Guid farmerId)
    {
        CustomerId = customerId;
        FarmerId = farmerId;
    }

    public Guid CustomerId { get; private set; }
    public Guid FarmerId { get; private set; }
}

public sealed class ProductFavorite : AuditableEntity
{
    private ProductFavorite() { }
    public ProductFavorite(Guid customerId, Guid productId)
    {
        CustomerId = customerId;
        ProductId = productId;
    }

    public Guid CustomerId { get; private set; }
    public Guid ProductId { get; private set; }
}
