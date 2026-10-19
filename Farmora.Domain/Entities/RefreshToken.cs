using Farmora.Domain.Common;

namespace Farmora.Domain.Entities;

public sealed class RefreshToken : AuditableEntity
{
    private RefreshToken() { }
    public RefreshToken(Guid userId, string tokenHash, DateTime expiresAtUtc)
    {
        UserId = userId;
        TokenHash = tokenHash;
        ExpiresAtUtc = expiresAtUtc;
    }

    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; } = null!;
    public DateTime ExpiresAtUtc { get; private set; }
    public DateTime? RevokedAtUtc { get; private set; }
    public string? ReplacedByHash { get; private set; }
    public User User { get; private set; } = null!;

    public bool IsActive => RevokedAtUtc is null && DateTime.UtcNow < ExpiresAtUtc;

    public void Revoke(string? replacedByHash = null)
    {
        RevokedAtUtc = DateTime.UtcNow;
        ReplacedByHash = replacedByHash;
        MarkUpdated();
    }
}
