using Farmora.Domain.Common;
namespace Farmora.Domain.Entities;
public sealed class Customer : AuditableEntity
{
    private Customer() { }
    public Customer(Guid userId) => UserId = userId;
    public Guid UserId { get; private set; }
    public string? DisplayName { get; private set; }
    public string? PreferencesJson { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAtUtc { get; private set; }
    public User User { get; private set; } = null!;
    public void UpdateProfile(string? displayName, string? preferencesJson) 
    {
        DisplayName = displayName?.Trim();
        PreferencesJson = preferencesJson; 

        MarkUpdated(); 
    }
    public void Deactivate() { IsDeleted = true; DeletedAtUtc = DateTime.UtcNow; MarkUpdated(); }
}
