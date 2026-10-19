using Farmora.Application.Common;
using Farmora.Application.DTO.Cart;
using Farmora.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Farmora.APi.Controllers;

[ApiController]
[Route("api/v1/cart")]
[Authorize]
public sealed class CartController(ICartService cart, ICurrentUser currentUser) : ControllerBase
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

    [HttpGet]
    [ProducesResponseType(typeof(CartResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CartResponse>> Get(CancellationToken ct)
    {
        if (RequireUser(out var userId) is { } err) return err;
        var result = await cart.GetAsync(userId, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("items")]
    [ProducesResponseType(typeof(CartResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CartResponse>> AddItem(AddCartItemDto dto, CancellationToken ct)
    {
        if (RequireUser(out var userId) is { } err) return err;
        return Ok(await cart.AddItemAsync(userId, dto, ct));
    }

    [HttpPut("items/{itemId:guid}")]
    [ProducesResponseType(typeof(CartResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CartResponse>> UpdateItem(Guid itemId, UpdateCartItemDto dto, CancellationToken ct)
    {
        if (RequireUser(out var userId) is { } err) return err;
        var result = await cart.UpdateItemAsync(userId, itemId, dto, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpDelete("items/{itemId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveItem(Guid itemId, CancellationToken ct)
    {
        if (RequireUser(out var userId) is { } err) return err;
        return await cart.RemoveItemAsync(userId, itemId, ct) ? NoContent() : NotFound();
    }

    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Clear(CancellationToken ct)
    {
        if (RequireUser(out var userId) is { } err) return err;
        await cart.ClearAsync(userId, ct);
        return NoContent();
    }
}
