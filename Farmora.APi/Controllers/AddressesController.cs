using Farmora.Application.Common;
using Farmora.Application.DTO.Address;
using Farmora.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Farmora.APi.Controllers;

[ApiController]
[Route("api/v1/addresses")]
[Authorize]
public sealed class AddressesController(IAddressService addresses, ICurrentUser currentUser) : ControllerBase
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

    [HttpGet("me")]
    [ProducesResponseType(typeof(IReadOnlyList<AddressResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<AddressResponse>>> List(CancellationToken ct)
    {
        if (RequireUser(out var userId) is { } err) return err;
        return Ok(await addresses.ListAsync(userId, ct));
    }

    [HttpGet("me/{addressId:guid}")]
    [ProducesResponseType(typeof(AddressResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AddressResponse>> Get(Guid addressId, CancellationToken ct)
    {
        if (RequireUser(out var userId) is { } err) return err;
        var address = await addresses.GetAsync(userId, addressId, ct);
        return address is null ? NotFound() : Ok(address);
    }

    [HttpPost("me")]
    [ProducesResponseType(typeof(AddressResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AddressResponse>> Create(CreateAddressDto dto, CancellationToken ct)
    {
        if (RequireUser(out var userId) is { } err) return err;
        var created = await addresses.CreateAsync(userId, dto, ct);
        return CreatedAtAction(nameof(Get), new { addressId = created.Id }, created);
    }

    [HttpPut("me/{addressId:guid}")]
    [ProducesResponseType(typeof(AddressResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AddressResponse>> Update(Guid addressId, UpdateAddressDto dto, CancellationToken ct)
    {
        if (RequireUser(out var userId) is { } err) return err;
        var updated = await addresses.UpdateAsync(userId, addressId, dto, ct);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("me/{addressId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid addressId, CancellationToken ct)
    {
        if (RequireUser(out var userId) is { } err) return err;
        return await addresses.DeleteAsync(userId, addressId, ct) ? NoContent() : NotFound();
    }

    [HttpPost("me/{addressId:guid}/default")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetDefault(Guid addressId, CancellationToken ct)
    {
        if (RequireUser(out var userId) is { } err) return err;
        return await addresses.SetDefaultAsync(userId, addressId, ct) ? NoContent() : NotFound();
    }
}
