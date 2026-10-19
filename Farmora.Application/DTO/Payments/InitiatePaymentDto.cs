namespace Farmora.Application.DTO.Payments;

public sealed class InitiatePaymentDto
{
    public Guid OrderId { get; init; }
    public string Provider { get; init; } = "Stub";
}
