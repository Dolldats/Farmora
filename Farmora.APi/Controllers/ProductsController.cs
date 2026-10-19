using Farmora.Application.Common;
using Farmora.Application.DTO.Catalog;
using Farmora.Application.DTO.Files;
using Farmora.Application.Interfaces.Services;
using Farmora.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Farmora.APi.Controllers;

[ApiController]
[Route("api/v1/products")]
public sealed class ProductsController(
    IProductService products,
    IFileService files,
    ICurrentUser currentUser) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search([FromQuery] ProductSearchParams query, CancellationToken ct)
    {
        var (items, total) = await products.SearchAsync(query ?? new ProductSearchParams(), ct);
        return Ok(new { items, total, page = query?.Page ?? 1, pageSize = query?.PageSize ?? 20 });
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductResponse>> GetById(Guid id, CancellationToken ct)
    {
        var product = await products.GetByIdAsync(id, ct);
        return product is null ? NotFound() : Ok(product);
    }

    [HttpPost("farmer")]
    [Authorize(Policy = "FarmerOnly")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductResponse>> Create(CreateProductDto dto, CancellationToken ct)
    {
        if (currentUser.UserId is not { } userId) return Unauthorized();
        var created = await products.CreateAsync(userId, dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("farmer/{id:guid}")]
    [Authorize(Policy = "FarmerOnly")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductResponse>> Update(Guid id, UpdateProductDto dto, CancellationToken ct)
    {
        if (currentUser.UserId is not { } userId) return Unauthorized();
        var updated = await products.UpdateAsync(userId, id, dto, ct);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("farmer/{id:guid}")]
    [Authorize(Policy = "FarmerOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        if (currentUser.UserId is not { } userId) return Unauthorized();
        return await products.DeleteAsync(userId, id, ct) ? NoContent() : NotFound();
    }

    [HttpPost("farmer/{id:guid}/stock")]
    [Authorize(Policy = "FarmerOnly")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductResponse>> AdjustStock(Guid id, [FromQuery] decimal delta, CancellationToken ct)
    {
        if (currentUser.UserId is not { } userId) return Unauthorized();
        var updated = await products.AdjustStockAsync(userId, id, delta, ct);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpPut("{id:guid}/status")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductResponse>> SetStatus(Guid id, [FromQuery] ProductStatus status, CancellationToken ct)
    {
        var updated = await products.SetStatusAsync(id, status, ct);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpPost("farmer/{id:guid}/images")]
    [Authorize(Policy = "FarmerOnly")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(FileUploadResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<FileUploadResponse>> UploadImage(Guid id, IFormFile file, CancellationToken ct)
    {
        if (currentUser.UserId is null) return Unauthorized();
        if (file is null || file.Length == 0) return BadRequest("An image file is required.");
        await using var stream = file.OpenReadStream();
        var uploaded = await files.SaveProductImageAsync(stream, file.FileName, file.ContentType ?? "application/octet-stream", ct);
        return Created(string.Empty, uploaded);
    }
}
