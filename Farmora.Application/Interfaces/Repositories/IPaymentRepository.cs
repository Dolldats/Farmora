using Farmora.Domain.Entities;

namespace Farmora.Application.Interfaces.Repositories;

public interface IPaymentRepository
{
    Task<Payment?> GetByOrderAsync(Guid orderId, CancellationToken cancellationToken);
    Task<Payment?> GetByReferenceAsync(string providerReference, CancellationToken cancellationToken);
    Task<bool> EventExistsAsync(string eventId, CancellationToken cancellationToken);
    Task AddAsync(Payment payment, CancellationToken cancellationToken);
    Task AddEventAsync(PaymentEvent paymentEvent, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
