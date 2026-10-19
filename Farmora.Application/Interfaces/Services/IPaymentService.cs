using Farmora.Application.DTO.Payments;

namespace Farmora.Application.Interfaces.Services;

public interface IPaymentService
{
    Task<PaymentResponse> InitiateAsync(Guid customerId, InitiatePaymentDto dto, CancellationToken cancellationToken);
    Task<PaymentResponse?> HandleWebhookAsync(WebhookDto dto, CancellationToken cancellationToken);
    Task<PaymentResponse?> GetByOrderAsync(Guid customerId, Guid orderId, CancellationToken cancellationToken);
    Task<PaymentResponse?> RefundAsync(Guid customerId, Guid orderId, CancellationToken cancellationToken);
}
