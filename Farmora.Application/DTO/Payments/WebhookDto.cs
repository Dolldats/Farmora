namespace Farmora.Application.DTO.Payments;

public sealed class WebhookDto
{
    public string EventId { get; init; } = string.Empty;
    public string EventType { get; init; } = string.Empty;
    public string ProviderReference { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string Signature { get; init; } = string.Empty;
    public string Payload { get; init; } = string.Empty;
}
