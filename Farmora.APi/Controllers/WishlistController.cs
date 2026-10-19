using Farmora.Application.Common;
using Farmora.Application.DTO.Wishlist;
using Farmora.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Farmora.APi.Controllers;

[ApiController]
[Route("api/v1/wishlist")]
[Authorize]
public sealed class WishlistController(IWishlistService wishlist, ICurrentUser currentUser) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<WishlistResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<WishlistResponse>>> List(CancellationToken ct)
    {
        if (currentUser.UserId is not { } userId) return Unauthorized();
        return Ok(await wishlist.ListAsync(userId, ct));
    }

    [HttpPost]
    [ProducesResponseType(typeof(WishlistResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<WishlistResponse>> Add(AddWishlistDto dto, CancellationToken ct)
    {
        if (currentUser.UserId is not { } userId) return Unauthorized();
        var added = await wishlist.AddAsync(userId, dto, ct);
        return CreatedAtAction(nameof(List), added);
    }

    [HttpDelete("{productId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remove(Guid productId, CancellationToken ct)
    {
        if (currentUser.UserId is not { } userId) return Unauthorized();
        return await wishlist.RemoveAsync(userId, productId, ct) ? NoContent() : NotFound();
    }
}
