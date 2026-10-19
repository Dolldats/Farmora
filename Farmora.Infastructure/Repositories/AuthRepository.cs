using Farmora.Application.Interfaces.Repositories;
using Farmora.Domain.Entities;
using Farmora.Infastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Farmora.Infastructure.Repositories;

public sealed class AuthRepository(FarmoraDbContext db) : IAuthRepository
{
    public Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return db.Users.SingleOrDefaultAsync(x => x.Email == email, cancellationToken);
    }

    public Task<User?> GetUserByEmailWithRolesAsync(string email, CancellationToken cancellationToken = default)
    {
        return db.Users
            .Include(x => x.UserRoles)
            .ThenInclude(x => x.Role)
            .SingleOrDefaultAsync(x => x.Email == email, cancellationToken);
    }

    public Task<User?> GetUserByIdWithRolesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return db.Users
            .Include(x => x.UserRoles)
            .ThenInclude(x => x.Role)
            .SingleOrDefaultAsync(x => x.Id == userId, cancellationToken);
    }

    public Task AddUserAsync(User user, CancellationToken cancellationToken = default)
    {
        return db.Users.AddAsync(user, cancellationToken).AsTask();
    }

    public Task<RefreshToken?> GetRefreshTokenByHashAsync(string tokenHash, CancellationToken cancellationToken = default)
    {
        return db.RefreshTokens.SingleOrDefaultAsync(x => x.TokenHash == tokenHash, cancellationToken);
    }

    public Task AddRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        return db.RefreshTokens.AddAsync(refreshToken, cancellationToken).AsTask();
    }

    public Task RevokeRefreshTokenAsync(RefreshToken refreshToken, string? replacedByHash = null, CancellationToken cancellationToken = default)
    {
        refreshToken.Revoke(replacedByHash);
        return Task.CompletedTask;
    }

    public async Task RevokeAllUserRefreshTokensAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var tokens = await db.RefreshTokens
            .Where(x => x.UserId == userId && x.RevokedAtUtc == null)
            .ToListAsync(cancellationToken);
        foreach (var token in tokens)
            token.Revoke();
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return db.SaveChangesAsync(cancellationToken);
    }
}
