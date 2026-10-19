using Farmora.Domain.Enums;

namespace Farmora.Application.DTO.Payments;

public sealed class PaymentResponse
{
    public Guid Id { get; init; }
    public Guid OrderId { get; init; }
    public string Provider { get; init; } = string.Empty;
    public string ProviderReference { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "NGN";
    public PaymentStatus Status { get; init; }
    public DateTime CreatedAtUtc { get; init; }
}
