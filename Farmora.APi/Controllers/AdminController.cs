using Farmora.Application.Common;
using Farmora.Application.DTO.Admin;
using Farmora.Application.DTO.Analytics;
using Farmora.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Farmora.APi.Controllers;

[ApiController]
[Route("api/v1/admin")]
[Authorize(Policy = "AdminOnly")]
public sealed class AdminController(IAdminService admin, IDashboardService dashboards) : ControllerBase
{
    [HttpGet("users")]
    [ProducesResponseType(typeof(PagedResult<UserAdminDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<UserAdminDto>>> GetUsers(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null, CancellationToken ct = default)
        => Ok(await admin.GetUsersAsync(new PaginationParams { Page = page, PageSize = pageSize }, search, ct));

    [HttpPut("users/{id:guid}/roles")]
    [ProducesResponseType(typeof(UserAdminDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserAdminDto>> SetRole(Guid id, SetRoleRequest request, CancellationToken ct)
    {
        var user = await admin.SetRoleAsync(id, request, ct);
        return user is null ? NotFound() : Ok(user);
    }

    [HttpPut("users/{id:guid}/status")]
    [ProducesResponseType(typeof(UserAdminDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserAdminDto>> SetStatus(Guid id, SetUserStatusRequest request, CancellationToken ct)
    {
        var user = await admin.SetUserStatusAsync(id, request, ct);
        return user is null ? NotFound() : Ok(user);
    }

    [HttpGet("audit")]
    [ProducesResponseType(typeof(PagedResult<AuditLogDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<AuditLogDto>>> GetAudit(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
        => Ok(await admin.GetAuditLogsAsync(new PaginationParams { Page = page, PageSize = pageSize }, ct));

    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(AdminDashboardDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<AdminDashboardDto>> GetDashboard(CancellationToken ct)
        => Ok(await dashboards.GetAdminDashboardAsync(ct));
}
