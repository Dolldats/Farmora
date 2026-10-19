using Farmora.Domain.Enums;

namespace Farmora.Application.DTO.Reports;

public sealed class CreateReportDto
{
    public string TargetType { get; set; } = string.Empty;
    public Guid TargetId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? EvidenceUrl { get; set; }
}

public sealed class AssignReportDto
{
    public Guid AdminId { get; set; }
}

public sealed class ResolveReportDto
{
    public string Resolution { get; set; } = string.Empty;
    public bool Dismissed { get; set; }
}

public sealed class ReportResponse
{
    public Guid Id { get; init; }
    public Guid ReporterId { get; init; }
    public string TargetType { get; init; } = string.Empty;
    public Guid TargetId { get; init; }
    public string Reason { get; init; } = string.Empty;
    public string? EvidenceUrl { get; init; }
    public string Status { get; init; } = string.Empty;
    public Guid? AssignedTo { get; init; }
    public string? Resolution { get; init; }
    public DateTime CreatedAtUtc { get; init; }
}
