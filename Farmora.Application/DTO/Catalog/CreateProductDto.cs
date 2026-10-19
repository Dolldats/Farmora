using Farmora.Domain.Enums;

namespace Farmora.Application.DTO.Catalog;

public sealed class CreateProductDto
{
    public Guid CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public ProductUnit Unit { get; set; }
    public decimal Stock { get; set; }
}
