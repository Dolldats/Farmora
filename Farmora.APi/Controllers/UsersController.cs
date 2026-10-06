using Farmora.Application.Users;
using Microsoft.AspNetCore.Mvc;

namespace Farmora.APi.Controllers;

[ApiController]
[Route("api/v1/users")]
public sealed class UsersController(IUserService service) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<UserResponse>> Create(CreateUserRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await service.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(Get), new { id = user.Id }, user);
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserResponse>> Get(Guid id, CancellationToken cancellationToken)
        => (await service.GetAsync(id, cancellationToken)) is { } user ? Ok(user) : NotFound();

    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<UserResponse>> Update(Guid id, UpdateUserRequest request, CancellationToken cancellationToken)
        => (await service.UpdateAsync(id, request, cancellationToken)) is { } user ? Ok(user) : NotFound();

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Disable(Guid id, CancellationToken cancellationToken)
        => await service.DisableAsync(id, cancellationToken) ? NoContent() : NotFound();
}
