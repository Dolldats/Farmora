using Farmora.Domain.Common;
using Farmora.Domain.Enums;
namespace Farmora.Domain.Entities;
public sealed class Farmer : AuditableEntity
{
    private Farmer() { }
    public Farmer(Guid userId, string farmName) 
    { 
        UserId = userId;
        FarmName = farmName.Trim();
    }

    public Guid UserId { get; private set; }
    public string FarmName { get; private set; } = null!;
    public string? Description { get; private set; }
    public string? Address { get; private set; }
    public decimal? Latitude { get; private set; }
    public decimal? Longitude { get; private set; }
    public FarmerVerificationStatus VerificationStatus { get; private set; } = FarmerVerificationStatus.Pending;
    public FarmerOperationalStatus OperationalStatus { get; private set; } = FarmerOperationalStatus.Inactive;
    public User User { get; private set; } = null!;
    public void SetVerification(FarmerVerificationStatus status)
    { 
        VerificationStatus = status;
        MarkUpdated(); 
    }
    public void SetOperationalStatus(FarmerOperationalStatus status)
    { 
        OperationalStatus = status;
        MarkUpdated();
    }
}
