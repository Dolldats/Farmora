using Farmora.Application.DTO.Catalog;
using Farmora.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Farmora.APi.Controllers;

[ApiController]
[Route("api/v1/search")]
public sealed class SearchController(IProductService products) : ControllerBase
{
    [HttpGet("products")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchProducts([FromQuery] ProductSearchParams query, CancellationToken ct)
    {
        var (items, total) = await products.SearchAsync(query ?? new ProductSearchParams(), ct);
        return Ok(new { items, total, page = query?.Page ?? 1, pageSize = query?.PageSize ?? 20 });
    }
}
