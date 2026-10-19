using Farmora.Application.Common;
using Farmora.Application.DTO.Orders;
using Farmora.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Farmora.APi.Controllers;

[ApiController]
[Route("api/v1/orders")]
[Authorize]
public sealed class OrdersController(IOrderService orders, ICurrentUser currentUser) : ControllerBase
{
    private ActionResult? RequireUser(out Guid userId)
    {
        if (currentUser.UserId is { } id)
        {
            userId = id;
            return null;
        }
        userId = Guid.Empty;
        return Unauthorized();
    }

    [HttpPost("quote")]
    [ProducesResponseType(typeof(QuoteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<QuoteResponse>> Quote(QuoteRequest request, CancellationToken ct)
    {
        if (RequireUser(out var userId) is { } err) return err;
        return Ok(await orders.QuoteAsync(userId, request, ct));
    }

    [HttpPost]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<OrderResponse>> Create(
        CreateOrderDto dto,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        CancellationToken ct)
    {
        if (RequireUser(out var userId) is { } err) return err;
        if (string.IsNullOrWhiteSpace(dto.IdempotencyKey) && !string.IsNullOrWhiteSpace(idempotencyKey))
            dto = new CreateOrderDto
            {
                AddressId = dto.AddressId,
                PromoCode = dto.PromoCode,
                DeliveryMethod = dto.DeliveryMethod,
                IdempotencyKey = idempotencyKey!,
            };
        var created = await orders.CreateAsync(userId, dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpGet("me")]
    [ProducesResponseType(typeof(PagedResult<OrderResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PagedResult<OrderResponse>>> GetMine(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        if (RequireUser(out var userId) is { } err) return err;
        return Ok(await orders.GetMyOrdersAsync(userId, page, pageSize, ct));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderResponse>> GetById(Guid id, CancellationToken ct)
    {
        if (RequireUser(out var userId) is { } err) return err;
        var order = await orders.GetByIdAsync(userId, id, ct);
        return order is null ? NotFound() : Ok(order);
    }

    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
    {
        if (RequireUser(out var userId) is { } err) return err;
        return await orders.CancelAsync(userId, id, ct) ? NoContent() : NotFound();
    }
}
