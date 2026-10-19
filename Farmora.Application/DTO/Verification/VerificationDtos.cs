using Farmora.Domain.Enums;

namespace Farmora.Application.DTO.Verification;

public sealed class VerificationDocumentInput
{
    public VerificationDocumentType DocumentType { get; set; }
    public string StorageKey { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}

public sealed class SubmitVerificationDto
{
    public Guid FarmerId { get; set; }
    public List<VerificationDocumentInput> Documents { get; set; } = [];
}

public sealed class DecideVerificationDto
{
    public FarmerVerificationStatus Status { get; set; }
    public string? DecisionNote { get; set; }
    public DateTime? ExpiresAtUtc { get; set; }
}

public sealed class VerificationDocumentDto
{
    public Guid Id { get; init; }
    public string DocumentType { get; init; } = string.Empty;
    public string StorageKey { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;
}

public sealed class VerificationResponse
{
    public Guid Id { get; init; }
    public Guid FarmerId { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? DecisionNote { get; init; }
    public Guid? DecidedBy { get; init; }
    public DateTime? DecidedAtUtc { get; init; }
    public DateTime? ExpiresAtUtc { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public IReadOnlyList<VerificationDocumentDto> Documents { get; init; } = [];
}
