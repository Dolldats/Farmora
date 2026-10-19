using Farmora.Application.Interfaces.Repositories;
using Farmora.Domain.Entities;
using Farmora.Infastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Farmora.Infastructure.Repositories;

public sealed class PaymentRepository(FarmoraDbContext db) : IPaymentRepository
{
    public Task<Payment?> GetByOrderAsync(Guid orderId, CancellationToken cancellationToken) =>
        db.Set<Payment>()
            .Include(p => p.Events)
            .FirstOrDefaultAsync(p => p.OrderId == orderId, cancellationToken);

    public Task<Payment?> GetByReferenceAsync(string providerReference, CancellationToken cancellationToken) =>
        db.Set<Payment>()
            .Include(p => p.Events)
            .FirstOrDefaultAsync(p => p.ProviderReference == providerReference, cancellationToken);

    public Task<bool> EventExistsAsync(string eventId, CancellationToken cancellationToken) =>
        db.Set<PaymentEvent>().AsNoTracking().AnyAsync(e => e.EventId == eventId, cancellationToken);

    public Task AddAsync(Payment payment, CancellationToken cancellationToken) =>
        db.Set<Payment>().AddAsync(payment, cancellationToken).AsTask();

    public Task AddEventAsync(PaymentEvent paymentEvent, CancellationToken cancellationToken) =>
        db.Set<PaymentEvent>().AddAsync(paymentEvent, cancellationToken).AsTask();

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        db.SaveChangesAsync(cancellationToken);
}
