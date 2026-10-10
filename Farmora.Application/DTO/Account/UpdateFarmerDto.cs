namespace Farmora.Application.DTO.Account;

public sealed class UpdateFarmerDto
{
    public string FarmName { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? Address { get; init; }
    public decimal? Latitude { get; init; }
    public decimal? Longitude { get; init; }
}
