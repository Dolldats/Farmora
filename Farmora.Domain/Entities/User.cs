using Farmora.Domain.Common;
using Farmora.Domain.Enums;
namespace Farmora.Domain.Entities;
public sealed class User : AuditableEntity
{
    private readonly List<UserRole> _userRoles = [];
    private User() { }
    public User(string email, string? phone, string passwordHash) { Email = email.Trim().ToLowerInvariant(); Phone = phone?.Trim(); PasswordHash = passwordHash; }
    public string Email { get; private set; } = null!;
    public string? Phone { get; private set; }
    public string PasswordHash { get; private set; } = null!;
    public UserStatus Status { get; private set; } = UserStatus.Pending;
    public Customer? Customer { get; private set; }
    public Farmer? Farmer { get; private set; }
    public IReadOnlyCollection<UserRole> UserRoles => _userRoles;
    public void SetCustomer(Customer customer) => Customer = customer;
    public void SetFarmer(Farmer farmer) => Farmer = farmer;
    public void UpdatePhone(string? phone) { Phone = phone?.Trim(); MarkUpdated(); }
    public void UpdateProfile(string email, string? phone, string passwordHash)
    {
        Email = email.Trim().ToLowerInvariant();
        Phone = phone?.Trim();
        PasswordHash = passwordHash;
        MarkUpdated();
    }
    public void Disable() { Status = UserStatus.Disabled; MarkUpdated(); }
    public void Activate() { Status = UserStatus.Active; MarkUpdated(); }
}
