using Farmora.Application.Common;
using Farmora.Application.DTO.Files;
using Farmora.Application.DTO.Verification;
using Farmora.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Farmora.APi.Controllers;

[ApiController]
[Route("api/v1/verifications")]
[Authorize]
public sealed class VerificationsController(
    IVerificationService verifications,
    IFileService files,
    ICurrentUser currentUser) : ControllerBase
{
    [HttpGet("mine")]
    [ProducesResponseType(typeof(VerificationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VerificationResponse>> GetMine(CancellationToken ct)
    {
        if (currentUser.UserId is not { } userId) return Unauthorized();
        var status = await verifications.GetMyStatusAsync(userId, ct);
        return status is null ? NotFound() : Ok(status);
    }

    [HttpPost("submit")]
    [Authorize(Policy = "FarmerOnly")]
    [ProducesResponseType(typeof(VerificationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VerificationResponse>> Submit(SubmitVerificationDto dto, CancellationToken ct)
    {
        if (currentUser.UserId is not { } userId) return Unauthorized();
        dto.FarmerId = userId;
        var created = await verifications.SubmitAsync(dto, ct);
        return CreatedAtAction(nameof(GetMine), created);
    }

    [HttpPost("documents")]
    [Authorize(Policy = "FarmerOnly")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(FileUploadResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<FileUploadResponse>> UploadDocument(IFormFile file, CancellationToken ct)
    {
        if (currentUser.UserId is null) return Unauthorized();
        if (file is null || file.Length == 0) return BadRequest("A document file is required.");
        await using var stream = file.OpenReadStream();
        var uploaded = await files.SaveVerificationDocumentAsync(stream, file.FileName, file.ContentType ?? "application/octet-stream", ct);
        return Created(string.Empty, uploaded);
    }

    [HttpGet("queue")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(PagedResult<VerificationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<VerificationResponse>>> GetQueue(
        [FromQuery] string? status = null, [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20, CancellationToken ct = default)
        => Ok(await verifications.GetQueueAsync(status, page, pageSize, ct));

    [HttpPost("{id:guid}/decide")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(VerificationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VerificationResponse>> Decide(Guid id, DecideVerificationDto dto, CancellationToken ct)
    {
        if (currentUser.UserId is not { } adminId) return Unauthorized();
        var decided = await verifications.DecideAsync(id, adminId, dto, ct);
        return decided is null ? NotFound() : Ok(decided);
    }
}
