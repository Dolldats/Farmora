namespace Farmora.Application.DTO.Admin;

public sealed class AuditLogDto
{
    public Guid Id { get; init; }
    public Guid? ActorId { get; init; }
    public string Action { get; init; } = string.Empty;
    public string Resource { get; init; } = string.Empty;
    public Guid? ResourceId { get; init; }
    public string? Detail { get; init; }
    public DateTime CreatedAtUtc { get; init; }
}
