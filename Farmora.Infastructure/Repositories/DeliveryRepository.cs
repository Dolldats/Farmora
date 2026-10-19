using Farmora.Application.Interfaces.Repositories;
using Farmora.Domain.Entities;
using Farmora.Infastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Farmora.Infastructure.Repositories;

public sealed class DeliveryRepository(FarmoraDbContext db) : IDeliveryRepository
{
    public Task<Delivery?> GetByIdAsync(Guid deliveryId, CancellationToken cancellationToken) =>
        db.Set<Delivery>()
            .Include(d => d.Timeline)
            .FirstOrDefaultAsync(d => d.Id == deliveryId, cancellationToken);

    public Task<Delivery?> GetByOrderAsync(Guid orderId, CancellationToken cancellationToken) =>
        db.Set<Delivery>()
            .Include(d => d.Timeline)
            .FirstOrDefaultAsync(d => d.OrderId == orderId, cancellationToken);

    public Task AddAsync(Delivery delivery, CancellationToken cancellationToken) =>
        db.Set<Delivery>().AddAsync(delivery, cancellationToken).AsTask();

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        db.SaveChangesAsync(cancellationToken);
}
