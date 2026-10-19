using Farmora.Domain.Common;
using Farmora.Domain.Enums;

namespace Farmora.Domain.Entities;

public sealed class FarmerVerification : AuditableEntity
{
    private readonly List<VerificationDocument> _documents = [];
    private FarmerVerification() { }
    public FarmerVerification(Guid farmerId)
    {
        FarmerId = farmerId;
        Status = FarmerVerificationStatus.Pending;
    }

    public Guid FarmerId { get; private set; }
    public FarmerVerificationStatus Status { get; private set; }
    public string? DecisionNote { get; private set; }
    public Guid? DecidedBy { get; private set; }
    public DateTime? DecidedAtUtc { get; private set; }
    public DateTime? ExpiresAtUtc { get; private set; }
    public Farmer Farmer { get; private set; } = null!;
    public IReadOnlyCollection<VerificationDocument> Documents => _documents;

    public void AddDocument(VerificationDocument doc) => _documents.Add(doc);

    public void Decide(FarmerVerificationStatus status, Guid adminId, string? note, DateTime? expiry = null)
    {
        Status = status;
        DecidedBy = adminId;
        DecisionNote = note;
        DecidedAtUtc = DateTime.UtcNow;
        ExpiresAtUtc = expiry;
        MarkUpdated();
    }
}

public sealed class VerificationDocument : Entity
{
    private VerificationDocument() { }
    public VerificationDocument(Guid verificationId, VerificationDocumentType type, string storageKey, string url)
    {
        VerificationId = verificationId;
        DocumentType = type;
        StorageKey = storageKey;
        Url = url;
    }

    public Guid VerificationId { get; private set; }
    public VerificationDocumentType DocumentType { get; private set; }
    public string StorageKey { get; private set; } = null!;
    public string Url { get; private set; } = null!;
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
}
