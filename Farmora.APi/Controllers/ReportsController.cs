using Farmora.Application.Common;
using Farmora.Application.DTO.Reports;
using Farmora.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Farmora.APi.Controllers;

[ApiController]
[Route("api/v1/reports")]
[Authorize]
public sealed class ReportsController(IReportService reports, ICurrentUser currentUser) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(ReportResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ReportResponse>> Create(CreateReportDto dto, CancellationToken ct)
    {
        if (currentUser.UserId is not { } userId) return Unauthorized();
        var created = await reports.CreateAsync(userId, dto, ct);
        return CreatedAtAction(nameof(GetMine), created);
    }

    [HttpGet("mine")]
    [ProducesResponseType(typeof(IReadOnlyList<ReportResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<ReportResponse>>> GetMine(CancellationToken ct)
    {
        if (currentUser.UserId is not { } userId) return Unauthorized();
        return Ok(await reports.GetMyReportsAsync(userId, ct));
    }

    [HttpGet]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(PagedResult<ReportResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ReportResponse>>> List(
        [FromQuery] string? status = null, [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20, CancellationToken ct = default)
        => Ok(await reports.ListAsync(status, page, pageSize, ct));

    [HttpPost("{id:guid}/assign")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ReportResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReportResponse>> Assign(Guid id, AssignReportDto dto, CancellationToken ct)
    {
        var assigned = await reports.AssignAsync(id, dto, ct);
        return assigned is null ? NotFound() : Ok(assigned);
    }

    [HttpPost("{id:guid}/resolve")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ReportResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReportResponse>> Resolve(Guid id, ResolveReportDto dto, CancellationToken ct)
    {
        var resolved = await reports.ResolveAsync(id, dto, ct);
        return resolved is null ? NotFound() : Ok(resolved);
    }
}
