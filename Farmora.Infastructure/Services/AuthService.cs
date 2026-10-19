using Farmora.Application.DTO.Auth;
using Farmora.Application.Interfaces.Services;
using Farmora.Domain.Entities;
using Farmora.Infastructure.Data;
using Farmora.Infastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Farmora.Infastructure.Services;

public sealed class AuthService(
    FarmoraDbContext db,
    IPasswordHasher<User> passwordHasher,
    JwtTokenService jwt,
    IOptions<JwtOptions> options) : IAuthService
{
    private const int MaxFailedAttempts = 5;
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            throw new ArgumentException("Email is required.");
        if (string.IsNullOrWhiteSpace(request.Password))
            throw new ArgumentException("Password is required.");

        var email = request.Email.Trim().ToLowerInvariant();
        if (await db.Users.AnyAsync(u => u.Email == email, cancellationToken))
            throw new InvalidOperationException("A user with this email already exists.");

        var roleName = string.IsNullOrWhiteSpace(request.Role) ? "Customer" : request.Role.Trim();
        if (!roleName.Equals("Customer", StringComparison.OrdinalIgnoreCase) &&
            !roleName.Equals("Farmer", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Role must be Customer or Farmer.");

        await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);

        var user = new User(email, request.Phone?.Trim(), string.Empty);
        user.UpdateProfile(email, request.Phone?.Trim(), passwordHasher.HashPassword(user, request.Password));
        user.Activate();
        db.Users.Add(user);
        await db.SaveChangesAsync(cancellationToken);

        var role = await db.Set<Role>().SingleOrDefaultAsync(r => r.Name == roleName, cancellationToken);
        if (role is null)
        {
            role = new Role(roleName);
            db.Set<Role>().Add(role);
            await db.SaveChangesAsync(cancellationToken);
        }
        db.Set<UserRole>().Add(new UserRole(user.Id, role.Id));

        if (roleName.Equals("Farmer", StringComparison.OrdinalIgnoreCase))
        {
            var local = email.Split('@')[0];
            db.Set<Farmer>().Add(new Farmer(user.Id, $"{local}'s Farm"));
        }
        else
        {
            db.Set<Customer>().Add(new Customer(user.Id));
        }

        var response = await IssueTokensAsync(user, cancellationToken);
        await tx.CommitAsync(cancellationToken);
        return response;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var email = (request.Email ?? string.Empty).Trim().ToLowerInvariant();
        var user = await db.Users.SingleOrDefaultAsync(u => u.Email == email, cancellationToken);
        if (user is null)
            throw new UnauthorizedAccessException("Invalid email or password.");

        if (user.LockoutEndUtc.HasValue && user.LockoutEndUtc.Value > DateTime.UtcNow)
            throw new UnauthorizedAccessException("Account is temporarily locked. Try again later.");

        var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
        {
            user.RecordFailedLogin(MaxFailedAttempts, LockoutDuration);
            await db.SaveChangesAsync(cancellationToken);
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        if (user.Status == Domain.Enums.UserStatus.Disabled)
            throw new UnauthorizedAccessException("Account is disabled.");

        user.ResetLockout();
        return await IssueTokensAsync(user, cancellationToken);
    }

    public async Task<AuthResponse> RefreshAsync(RefreshRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            throw new ArgumentException("Refresh token is required.");

        var hash = JwtTokenService.Hash(request.RefreshToken.Trim());
        var stored = await db.Set<RefreshToken>().SingleOrDefaultAsync(t => t.TokenHash == hash, cancellationToken);
        if (stored is null || !stored.IsActive)
            throw new UnauthorizedAccessException("Invalid or expired refresh token.");

        var user = await db.Users.SingleOrDefaultAsync(u => u.Id == stored.UserId, cancellationToken);
        if (user is null || user.Status == Domain.Enums.UserStatus.Disabled)
            throw new UnauthorizedAccessException("Account is disabled.");
        if (user.LockoutEndUtc.HasValue && user.LockoutEndUtc.Value > DateTime.UtcNow)
            throw new UnauthorizedAccessException("Account is temporarily locked.");

        var (plain, newHash) = jwt.GenerateRefreshToken();
        stored.Revoke(newHash);
        db.Set<RefreshToken>().Add(new RefreshToken(
            user.Id, newHash, DateTime.UtcNow.AddDays(options.Value.RefreshDays)));

        var roles = await GetRolesAsync(user.Id, cancellationToken);
        var (access, expires) = jwt.GenerateAccessToken(user, roles);
        await db.SaveChangesAsync(cancellationToken);

        return new AuthResponse
        {
            AccessToken = access,
            RefreshToken = plain,
            ExpiresAtUtc = expires,
            User = new AuthUserDto { Id = user.Id, Email = user.Email, Roles = roles },
        };
    }

    public async Task LogoutAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            return;
        var hash = JwtTokenService.Hash(refreshToken.Trim());
        var stored = await db.Set<RefreshToken>().SingleOrDefaultAsync(t => t.TokenHash == hash, cancellationToken);
        if (stored is null || !stored.IsActive)
            return;
        stored.Revoke();
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task ConfirmEmailAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await db.Users.SingleOrDefaultAsync(u => u.Id == userId, cancellationToken)
            ?? throw new KeyNotFoundException("User not found.");
        user.ConfirmEmail();
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task<AuthResponse> IssueTokensAsync(User user, CancellationToken ct)
    {
        var roles = await GetRolesAsync(user.Id, ct);
        var (access, expires) = jwt.GenerateAccessToken(user, roles);
        var (plain, hash) = jwt.GenerateRefreshToken();
        db.Set<RefreshToken>().Add(new RefreshToken(
            user.Id, hash, DateTime.UtcNow.AddDays(options.Value.RefreshDays)));
        await db.SaveChangesAsync(ct);
        return new AuthResponse
        {
            AccessToken = access,
            RefreshToken = plain,
            ExpiresAtUtc = expires,
            User = new AuthUserDto { Id = user.Id, Email = user.Email, Roles = roles },
        };
    }

    private async Task<IReadOnlyList<string>> GetRolesAsync(Guid userId, CancellationToken ct) =>
        await db.Set<UserRole>().AsNoTracking()
            .Where(ur => ur.UserId == userId)
            .Join(db.Set<Role>().AsNoTracking(), ur => ur.RoleId, r => r.Id, (_, r) => r.Name)
            .ToListAsync(ct);
}
