namespace Farmora.Application.DTO.Catalog;

public enum ProductSort
{
    Newest,
    PriceAsc,
    PriceDesc,
    Rating
}

public sealed class ProductSearchParams
{
    public string? Keyword { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? FarmerId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public bool? OnlyAvailable { get; set; }
    public double? MinRating { get; set; }
    public ProductSort Sort { get; set; } = ProductSort.Newest;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
