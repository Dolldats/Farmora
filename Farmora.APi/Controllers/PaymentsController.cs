using Farmora.Application.Common;
using Farmora.Application.DTO.Payments;
using Farmora.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Farmora.APi.Controllers;

[ApiController]
[Authorize]
public sealed class PaymentsController(IPaymentService payments, ICurrentUser currentUser) : ControllerBase
{
    [HttpPost("api/v1/orders/{id:guid}/payments")]
    [ProducesResponseType(typeof(PaymentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PaymentResponse>> Initiate(Guid id, InitiatePaymentDto dto, CancellationToken ct)
    {
        if (currentUser.UserId is not { } userId) return Unauthorized();
        var effective = dto.OrderId == Guid.Empty
            ? new InitiatePaymentDto { OrderId = id, Provider = dto.Provider }
            : dto;
        return Ok(await payments.InitiateAsync(userId, effective, ct));
    }

    [HttpGet("api/v1/orders/{id:guid}/payments")]
    [ProducesResponseType(typeof(PaymentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaymentResponse>> GetByOrder(Guid id, CancellationToken ct)
    {
        if (currentUser.UserId is not { } userId) return Unauthorized();
        var payment = await payments.GetByOrderAsync(userId, id, ct);
        return payment is null ? NotFound() : Ok(payment);
    }

    [HttpPost("api/v1/orders/{id:guid}/payments/refund")]
    [ProducesResponseType(typeof(PaymentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaymentResponse>> Refund(Guid id, CancellationToken ct)
    {
        if (currentUser.UserId is not { } userId) return Unauthorized();
        var payment = await payments.RefundAsync(userId, id, ct);
        return payment is null ? NotFound() : Ok(payment);
    }

    [HttpPost("api/v1/payments/webhooks")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PaymentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaymentResponse>> Webhook(
        WebhookDto dto,
        [FromHeader(Name = "X-Signature")] string? signature,
        CancellationToken ct)
    {
        var effective = !string.IsNullOrWhiteSpace(dto.Signature)
            ? dto
            : new WebhookDto
            {
                EventId = dto.EventId,
                EventType = dto.EventType,
                ProviderReference = dto.ProviderReference,
                Status = dto.Status,
                Signature = signature ?? string.Empty,
                Payload = dto.Payload,
            };
        var payment = await payments.HandleWebhookAsync(effective, ct);
        return payment is null ? NotFound() : Ok(payment);
    }
}
