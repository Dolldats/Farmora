using Farmora.Application.Common;
using Farmora.Application.DTO.Notifications;
using Farmora.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Farmora.APi.Controllers;

[ApiController]
[Route("api/v1/notifications")]
[Authorize]
public sealed class NotificationsController(INotificationService notifications, ICurrentUser currentUser) : ControllerBase
{
    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(NotificationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<NotificationResponse>> Create(CreateNotificationDto dto, CancellationToken ct)
    {
        var created = await notifications.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(List), created);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<NotificationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PagedResult<NotificationResponse>>> List(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        [FromQuery] bool unreadOnly = false, CancellationToken ct = default)
    {
        if (currentUser.UserId is not { } userId) return Unauthorized();
        return Ok(await notifications.ListAsync(userId, page, pageSize, unreadOnly, ct));
    }

    [HttpPost("{id:guid}/read")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkRead(Guid id, CancellationToken ct)
    {
        if (currentUser.UserId is not { } userId) return Unauthorized();
        return await notifications.MarkReadAsync(id, userId, ct) ? NoContent() : NotFound();
    }

    [HttpPost("read-all")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> MarkAllRead(CancellationToken ct)
    {
        if (currentUser.UserId is not { } userId) return Unauthorized();
        var count = await notifications.MarkAllReadAsync(userId, ct);
        return Ok(new { markedRead = count });
    }
}
