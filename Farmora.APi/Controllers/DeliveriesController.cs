using Farmora.Application.Common;
using Farmora.Application.DTO.Delivery;
using Farmora.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Farmora.APi.Controllers;

[ApiController]
[Route("api/v1/deliveries")]
[Authorize]
public sealed class DeliveriesController(IDeliveryService deliveries, ICurrentUser currentUser) : ControllerBase
{
    [HttpGet("orders/{orderId:guid}")]
    [ProducesResponseType(typeof(DeliveryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DeliveryResponse>> GetByOrder(Guid orderId, CancellationToken ct)
    {
        if (currentUser.UserId is not { } userId) return Unauthorized();
        var delivery = await deliveries.GetByOrderAsync(userId, orderId, ct);
        return delivery is null ? NotFound() : Ok(delivery);
    }

    [HttpPut("orders/{orderId:guid}")]
    [Authorize(Roles = "Admin,Farmer")]
    [ProducesResponseType(typeof(DeliveryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DeliveryResponse>> UpdateStatus(Guid orderId, UpdateDeliveryDto dto, CancellationToken ct)
    {
        var delivery = await deliveries.UpdateStatusAsync(orderId, dto, ct);
        return delivery is null ? NotFound() : Ok(delivery);
    }
}
