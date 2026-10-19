using Farmora.Application.Common;
using Farmora.Application.DTO.Analytics;
using Farmora.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Farmora.APi.Controllers;

[ApiController]
[Route("api/v1/analytics")]
[Authorize]
public sealed class AnalyticsController(
    IAnalyticsService analytics,
    IDashboardService dashboards,
    ICurrentUser currentUser) : ControllerBase
{
    [HttpPost("events")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Track(AnalyticsEventDto dto, CancellationToken ct)
    {
        dto.ActorId ??= currentUser.UserId;
        await analytics.TrackAsync(dto, ct);
        return NoContent();
    }

    [HttpGet("dashboards/customer")]
    [ProducesResponseType(typeof(CustomerDashboardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CustomerDashboardDto>> CustomerDashboard(CancellationToken ct)
    {
        if (currentUser.UserId is not { } userId) return Unauthorized();
        return Ok(await dashboards.GetCustomerDashboardAsync(userId, ct));
    }

    [HttpGet("dashboards/farmer")]
    [Authorize(Policy = "FarmerOnly")]
    [ProducesResponseType(typeof(FarmerDashboardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<FarmerDashboardDto>> FarmerDashboard(CancellationToken ct)
    {
        if (currentUser.UserId is not { } userId) return Unauthorized();
        return Ok(await dashboards.GetFarmerDashboardAsync(userId, ct));
    }

    [HttpGet("dashboards/admin")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(AdminDashboardDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<AdminDashboardDto>> AdminDashboard(CancellationToken ct)
        => Ok(await dashboards.GetAdminDashboardAsync(ct));
}
