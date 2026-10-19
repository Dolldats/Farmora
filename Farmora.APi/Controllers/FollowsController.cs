using Farmora.Application.Common;
using Farmora.Application.DTO.Following;
using Farmora.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Farmora.APi.Controllers;

[ApiController]
[Authorize]
public sealed class FollowsController(IFollowService follows, ICurrentUser currentUser) : ControllerBase
{
    [HttpPost("api/v1/farmers/{farmerId:guid}/follow")]
    [ProducesResponseType(typeof(FollowResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FollowResponse>> FollowFarmer(Guid farmerId, CancellationToken ct)
    {
        if (currentUser.UserId is not { } userId) return Unauthorized();
        var follow = await follows.FollowFarmerAsync(userId, farmerId, ct);
        return follow is null ? NotFound() : Ok(follow);
    }

    [HttpDelete("api/v1/farmers/{farmerId:guid}/follow")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UnfollowFarmer(Guid farmerId, CancellationToken ct)
    {
        if (currentUser.UserId is not { } userId) return Unauthorized();
        return await follows.UnfollowFarmerAsync(userId, farmerId, ct) ? NoContent() : NotFound();
    }

    [HttpGet("api/v1/follows/mine")]
    [ProducesResponseType(typeof(IReadOnlyList<FollowResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<FollowResponse>>> GetMyFollows(CancellationToken ct)
    {
        if (currentUser.UserId is not { } userId) return Unauthorized();
        return Ok(await follows.GetMyFollowsAsync(userId, ct));
    }

    [HttpPost("api/v1/products/{productId:guid}/favorite")]
    [ProducesResponseType(typeof(FavoriteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FavoriteResponse>> FavoriteProduct(Guid productId, CancellationToken ct)
    {
        if (currentUser.UserId is not { } userId) return Unauthorized();
        var favorite = await follows.FavoriteProductAsync(userId, productId, ct);
        return favorite is null ? NotFound() : Ok(favorite);
    }

    [HttpDelete("api/v1/products/{productId:guid}/favorite")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UnfavoriteProduct(Guid productId, CancellationToken ct)
    {
        if (currentUser.UserId is not { } userId) return Unauthorized();
        return await follows.UnfavoriteProductAsync(userId, productId, ct) ? NoContent() : NotFound();
    }

    [HttpGet("api/v1/favorites/mine")]
    [ProducesResponseType(typeof(IReadOnlyList<FavoriteResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<FavoriteResponse>>> GetMyFavorites(CancellationToken ct)
    {
        if (currentUser.UserId is not { } userId) return Unauthorized();
        return Ok(await follows.GetMyFavoritesAsync(userId, ct));
    }
}
