using Farmora.Application.DTO.User;
using Farmora.Application.Interfaces.Services;
using Farmora.Domain.Entities;
using Farmora.Infastructure.Data.Migrations;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Farmora.APi.Controllers;

[ApiController]
[Route("api/v1/users")]
public sealed class UsersController(IUserService service) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserResponse>> Create(CreateUserRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await service.CreateAsync(request, cancellationToken);
            return CreatedAtAction(
                nameof(Get), 
                new { id = user.Id },
                user
            );
        }
        catch (ArgumentException ex) 
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponse>> Get(Guid id, CancellationToken cancellationToken)
    {
        var user = await service.GetAsync(id, cancellationToken);

        if (user is null)
        {
            return NotFound();
        }

        return Ok(user);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<UserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<UserResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var users = await service.GetAllAsync(cancellationToken);
        return Ok(users);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponse>> Update(Guid id, UpdateUserRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await service.UpdateAsync(id, request, cancellationToken);

            if (user is null)
            {
                return NotFound();
            }

            return Ok(user);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Disable(Guid id, CancellationToken cancellationToken)
    {
        var disabled = await service.DisableAsync(id, cancellationToken);

        if (!disabled)
        {
            return NotFound();
        }

        return NoContent();
    }
}


