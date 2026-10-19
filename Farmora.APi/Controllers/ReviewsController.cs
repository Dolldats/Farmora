using Farmora.Application.Common;
using Farmora.Application.DTO.Reviews;
using Farmora.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Farmora.APi.Controllers;

[ApiController]
[Route("api/v1/reviews")]
public sealed class ReviewsController(IReviewService reviews, ICurrentUser currentUser) : ControllerBase
{
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(ReviewResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ReviewResponse>> Create(CreateReviewDto dto, CancellationToken ct)
    {
        if (currentUser.UserId is not { } userId) return Unauthorized();
        var created = await reviews.CreateAsync(userId, dto, ct);
        return CreatedAtAction(nameof(GetMine), created);
    }

    [HttpGet("mine")]
    [Authorize]
    [ProducesResponseType(typeof(IReadOnlyList<ReviewResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<ReviewResponse>>> GetMine(CancellationToken ct)
    {
        if (currentUser.UserId is not { } userId) return Unauthorized();
        return Ok(await reviews.GetMyReviewsAsync(userId, ct));
    }

    [HttpGet("product/{productId:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PagedResult<ReviewResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ReviewResponse>>> GetByProduct(
        Guid productId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
        => Ok(await reviews.GetByProductAsync(productId, page, pageSize, ct));

    [HttpPost("{id:guid}/moderate")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ReviewResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReviewResponse>> Moderate(Guid id, ModerateReviewDto dto, CancellationToken ct)
    {
        var moderated = await reviews.ModerateAsync(id, dto, ct);
        return moderated is null ? NotFound() : Ok(moderated);
    }
}
