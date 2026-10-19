using Farmora.Application.DTO.Payments;
using Farmora.Application.Interfaces.Repositories;
using Farmora.Application.Interfaces.Services;
using Farmora.Domain.Entities;
using Farmora.Domain.Enums;
using Farmora.Infastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Farmora.Infastructure.Services;

public sealed class PaymentService(
    IPaymentRepository payments,
    IOrderRepository orders,
    FarmoraDbContext db) : IPaymentService
{
    public async Task<PaymentResponse> InitiateAsync(Guid customerId, InitiatePaymentDto dto, CancellationToken cancellationToken)
    {
        var customer = await RequireCustomerAsync(customerId, cancellationToken);
        var order = await orders.GetByIdWithDetailsAsync(dto.OrderId, cancellationToken)
            ?? throw new InvalidOperationException("Order not found.");
        if (order.CustomerId != customer.Id)
            throw new InvalidOperationException("Order not found.");

        var existing = await payments.GetByOrderAsync(order.Id, cancellationToken);
        if (existing is not null)
            return Map(existing);

        var payment = new Payment(order.Id, order.Total, order.Currency);
        await payments.AddAsync(payment, cancellationToken);
        await payments.SaveChangesAsync(cancellationToken);
        return Map(payment);
    }

    public async Task<PaymentResponse?> HandleWebhookAsync(WebhookDto dto, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dto.Signature))
            throw new InvalidOperationException("Invalid webhook signature.");

        // Idempotent replay: event already processed.
        if (await payments.EventExistsAsync(dto.EventId, cancellationToken))
            return await payments.GetByReferenceAsync(dto.ProviderReference, cancellationToken) is { } replayed
                ? Map(replayed)
                : null;

        var payment = await payments.GetByReferenceAsync(dto.ProviderReference, cancellationToken);
        if (payment is null) return null;

        var (paymentStatus, orderStatus) = MapStatuses(dto.Status);
        payment.SetStatus(paymentStatus);
        await payments.AddEventAsync(
            new PaymentEvent(payment.Id, dto.EventId, dto.EventType, dto.Payload),
            cancellationToken);

        var order = await orders.GetByIdWithDetailsAsync(payment.OrderId, cancellationToken);
        order?.TransitionTo(orderStatus, $"Webhook {dto.EventType}: {dto.Status}");

        await payments.SaveChangesAsync(cancellationToken);
        return Map(payment);
    }

    public async Task<PaymentResponse?> GetByOrderAsync(Guid customerId, Guid orderId, CancellationToken cancellationToken)
    {
        var customer = await RequireCustomerAsync(customerId, cancellationToken);
        var order = await orders.GetByIdWithDetailsAsync(orderId, cancellationToken);
        if (order is null || order.CustomerId != customer.Id) return null;
        var payment = await payments.GetByOrderAsync(orderId, cancellationToken);
        return payment is null ? null : Map(payment);
    }

    public async Task<PaymentResponse?> RefundAsync(Guid customerId, Guid orderId, CancellationToken cancellationToken)
    {
        var customer = await RequireCustomerAsync(customerId, cancellationToken);
        var order = await orders.GetByIdWithDetailsAsync(orderId, cancellationToken);
        if (order is null || order.CustomerId != customer.Id) return null;
        var payment = await payments.GetByOrderAsync(orderId, cancellationToken);
        if (payment is null) return null;

        payment.SetStatus(PaymentStatus.Refunded);
        order.TransitionTo(OrderStatus.Refunded, "Refunded");
        await payments.SaveChangesAsync(cancellationToken);
        return Map(payment);
    }

    private static (PaymentStatus Payment, OrderStatus Order) MapStatuses(string status) =>
        status.Trim().ToLowerInvariant() switch
        {
            "paid" or "success" or "successful" or "captured" => (PaymentStatus.Captured, OrderStatus.Paid),
            "failed" or "failure" => (PaymentStatus.Failed, OrderStatus.Failed),
            "refunded" => (PaymentStatus.Refunded, OrderStatus.Refunded),
            _ => throw new InvalidOperationException($"Unknown webhook status '{status}'."),
        };

    private async Task<Customer> RequireCustomerAsync(Guid idOrUserId, CancellationToken ct) =>
        await db.Set<Customer>().FirstOrDefaultAsync(c => c.Id == idOrUserId, ct)
        ?? await db.Set<Customer>().FirstOrDefaultAsync(c => c.UserId == idOrUserId, ct)
        ?? throw new InvalidOperationException("Customer profile required.");

    private static PaymentResponse Map(Payment p) => new()
    {
        Id = p.Id,
        OrderId = p.OrderId,
        Provider = p.Provider,
        ProviderReference = p.ProviderReference,
        Amount = p.Amount,
        Currency = p.Currency,
        Status = p.Status,
        CreatedAtUtc = p.CreatedAtUtc,
    };
}
