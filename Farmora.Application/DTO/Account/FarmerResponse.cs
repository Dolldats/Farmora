using Farmora.Domain.Enums;

namespace Farmora.Application.DTO.Account;

public sealed class FarmerResponse
{
    public Guid UserId { get; init; }
    public string FarmName { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? Address { get; init; }
    public decimal? Latitude { get; init; }
    public decimal? Longitude { get; init; }
    public FarmerVerificationStatus VerificationStatus { get; init; }
    public FarmerOperationalStatus OperationalStatus { get; init; }
}
