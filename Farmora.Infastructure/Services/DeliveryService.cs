using Farmora.Application.DTO.Delivery;
using Farmora.Application.Interfaces.Repositories;
using Farmora.Application.Interfaces.Services;
using Farmora.Domain.Entities;
using Farmora.Infastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Farmora.Infastructure.Services;

public sealed class DeliveryService(
    IDeliveryRepository deliveries,
    IOrderRepository orders,
    FarmoraDbContext db) : IDeliveryService
{
    public async Task<DeliveryResponse?> GetByOrderAsync(Guid customerId, Guid orderId, CancellationToken cancellationToken)
    {
        var customer = await RequireCustomerAsync(customerId, cancellationToken);
        var order = await orders.GetByIdWithDetailsAsync(orderId, cancellationToken);
        if (order is null || order.CustomerId != customer.Id) return null;
        var delivery = await deliveries.GetByOrderAsync(orderId, cancellationToken);
        return delivery is null ? null : Map(delivery);
    }

    public async Task<DeliveryResponse?> UpdateStatusAsync(Guid orderId, UpdateDeliveryDto dto, CancellationToken cancellationToken)
    {
        var delivery = await deliveries.GetByOrderAsync(orderId, cancellationToken);
        if (delivery is null) return null;
        if (!string.IsNullOrWhiteSpace(dto.TrackingNumber))
            delivery.SetTracking(dto.TrackingNumber);
        delivery.UpdateStatus(dto.Status, dto.Note);
        await deliveries.SaveChangesAsync(cancellationToken);
        return Map(delivery);
    }

    private async Task<Customer> RequireCustomerAsync(Guid idOrUserId, CancellationToken ct) =>
        await db.Set<Customer>().FirstOrDefaultAsync(c => c.Id == idOrUserId, ct)
        ?? await db.Set<Customer>().FirstOrDefaultAsync(c => c.UserId == idOrUserId, ct)
        ?? throw new InvalidOperationException("Customer profile required.");

    private static DeliveryResponse Map(Delivery d) => new()
    {
        Id = d.Id,
        OrderId = d.OrderId,
        Method = d.Method,
        Fee = d.Fee,
        TrackingNumber = d.TrackingNumber,
        Status = d.Status,
    };
}
