using Farmora.Domain.Enums;

namespace Farmora.Application.DTO.Catalog;

public sealed class UpdateProductDto
{
    public Guid? CategoryId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public ProductUnit? Unit { get; set; }
    public decimal? Stock { get; set; }
    public bool? IsAvailable { get; set; }
}
