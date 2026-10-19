using Farmora.Application.Common;
using Farmora.Application.DTO.Messaging;
using Farmora.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Farmora.APi.Controllers;

[ApiController]
[Authorize]
public sealed class MessagingController(IMessagingService messaging, ICurrentUser currentUser) : ControllerBase
{
    [HttpPost("api/v1/conversations")]
    [ProducesResponseType(typeof(ConversationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ConversationResponse>> CreateConversation(CreateConversationDto dto, CancellationToken ct)
    {
        if (currentUser.UserId is not { } userId) return Unauthorized();
        var created = await messaging.CreateConversationAsync(userId, dto, ct);
        return CreatedAtAction(nameof(ReadConversation), new { id = created.Id }, created);
    }

    [HttpGet("api/v1/conversations/mine")]
    [ProducesResponseType(typeof(IReadOnlyList<ConversationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<ConversationResponse>>> ListMine(CancellationToken ct)
    {
        if (currentUser.UserId is not { } userId) return Unauthorized();
        return Ok(await messaging.ListMineAsync(userId, ct));
    }

    [HttpGet("api/v1/conversations/{id:guid}/messages")]
    [ProducesResponseType(typeof(IReadOnlyList<MessageResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<MessageResponse>>> ReadConversation(
        Guid id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        if (currentUser.UserId is not { } userId) return Unauthorized();
        return Ok(await messaging.ReadConversationAsync(userId, id, page, pageSize, ct));
    }

    [HttpPost("api/v1/messages")]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<MessageResponse>> Send(SendMessageDto dto, CancellationToken ct)
    {
        if (currentUser.UserId is not { } userId) return Unauthorized();
        var sent = await messaging.SendAsync(userId, dto, ct);
        return CreatedAtAction(nameof(ReadConversation), new { id = sent.ConversationId }, sent);
    }

    [HttpPost("api/v1/messages/{id:guid}/read")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkRead(Guid id, CancellationToken ct)
    {
        if (currentUser.UserId is not { } userId) return Unauthorized();
        return await messaging.MarkMessageReadAsync(userId, id, ct) ? NoContent() : NotFound();
    }
}
