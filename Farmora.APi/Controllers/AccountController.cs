using Farmora.Application.Common;
using Farmora.Application.DTO.Account;
using Farmora.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Farmora.APi.Controllers;

[ApiController]
[Route("api/v1")]
[Authorize]
public sealed class AccountController(IAccountService accounts, ICurrentUser currentUser) : ControllerBase
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

    [HttpGet("users/me")]
    [ProducesResponseType(typeof(MyProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MyProfileResponse>> GetMe(CancellationToken ct)
    {
        if (RequireUser(out var userId) is { } err) return err;
        var me = await accounts.GetMeAsync(userId, ct);
        return me is null ? NotFound() : Ok(me);
    }

    [HttpPut("users/me")]
    [ProducesResponseType(typeof(MyProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MyProfileResponse>> UpdateMe(UpdateMyProfileDto dto, CancellationToken ct)
    {
        if (RequireUser(out var userId) is { } err) return err;
        var me = await accounts.UpdateMeAsync(userId, dto, ct);
        return me is null ? NotFound() : Ok(me);
    }

    [HttpGet("customers/me")]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CustomerResponse>> GetMyCustomer(CancellationToken ct)
    {
        if (RequireUser(out var userId) is { } err) return err;
        var customer = await accounts.GetMyCustomerAsync(userId, ct);
        return customer is null ? NotFound() : Ok(customer);
    }

    [HttpPost("customers/me")]
    [HttpPut("customers/me")]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CustomerResponse>> UpsertMyCustomer(UpdateCustomerDto dto, CancellationToken ct)
    {
        if (RequireUser(out var userId) is { } err) return err;
        return Ok(await accounts.UpsertMyCustomerAsync(userId, dto, ct));
    }

    [HttpDelete("customers/me")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteMyCustomer(CancellationToken ct)
    {
        if (RequireUser(out var userId) is { } err) return err;
        return await accounts.DeleteMyCustomerAsync(userId, ct) ? NoContent() : NotFound();
    }

    [HttpGet("farmers/me")]
    [ProducesResponseType(typeof(FarmerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FarmerResponse>> GetMyFarmer(CancellationToken ct)
    {
        if (RequireUser(out var userId) is { } err) return err;
        var farmer = await accounts.GetMyFarmerAsync(userId, ct);
        return farmer is null ? NotFound() : Ok(farmer);
    }

    [HttpPost("farmers/me")]
    [HttpPut("farmers/me")]
    [ProducesResponseType(typeof(FarmerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<FarmerResponse>> UpsertMyFarmer(UpdateFarmerDto dto, CancellationToken ct)
    {
        if (RequireUser(out var userId) is { } err) return err;
        return Ok(await accounts.UpsertMyFarmerAsync(userId, dto, ct));
    }
}
