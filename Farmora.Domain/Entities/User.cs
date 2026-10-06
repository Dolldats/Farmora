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
    public UserStatus Status { get; private set; } = UserStatus.PendingVerification;
    public int SecurityStampVersion { get; private set; }
    public byte[] RowVersion { get; private set; } = [];
    public Customer? Customer { get; private set; }
    public Farmer? Farmer { get; private set; }
    public IReadOnlyCollection<UserRole> UserRoles => _userRoles;
    public void SetCustomer(Customer customer) => Customer = customer;
    public void SetFarmer(Farmer farmer) => Farmer = farmer;
    public void Disable() { Status = UserStatus.Disabled; SecurityStampVersion++; MarkUpdated(); }
    public void Activate() { Status = UserStatus.Active; MarkUpdated(); }
}
