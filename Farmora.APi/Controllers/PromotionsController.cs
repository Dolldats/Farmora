using Farmora.Application.DTO.Promotions;
using Farmora.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Farmora.APi.Controllers;

[ApiController]
[Route("api/v1/promotions")]
public sealed class PromotionsController(IPromotionService promotions) : ControllerBase
{
    [HttpPost("validate")]
    [Authorize]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Validate(ApplyPromoDto dto, CancellationToken ct)
    {
        var discount = await promotions.ValidateAsync(dto, ct);
        return Ok(new { code = dto.Code, discount });
    }

    [HttpGet]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(IReadOnlyList<PromotionResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PromotionResponse>>> List(CancellationToken ct)
        => Ok(await promotions.ListAsync(ct));

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(PromotionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PromotionResponse>> GetById(Guid id, CancellationToken ct)
    {
        var promo = await promotions.GetByIdAsync(id, ct);
        return promo is null ? NotFound() : Ok(promo);
    }

    [HttpGet("code/{code}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(PromotionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PromotionResponse>> GetByCode(string code, CancellationToken ct)
    {
        var promo = await promotions.GetByCodeAsync(code, ct);
        return promo is null ? NotFound() : Ok(promo);
    }

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(PromotionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PromotionResponse>> Create(CreatePromotionDto dto, CancellationToken ct)
    {
        var created = await promotions.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPost("{id:guid}/disable")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Disable(Guid id, CancellationToken ct)
        => await promotions.DisableAsync(id, ct) ? NoContent() : NotFound();
}
