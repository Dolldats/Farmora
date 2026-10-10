using Farmora.Domain.Entities;

namespace Farmora.Application.Interfaces.Repositories;

public interface IAuthRepository
{
    Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetUserByEmailWithRolesAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetUserByIdWithRolesAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddUserAsync(User user, CancellationToken cancellationToken = default);
    Task<RefreshToken?> GetRefreshTokenByHashAsync(string tokenHash, CancellationToken cancellationToken = default);
    Task AddRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
    Task RevokeRefreshTokenAsync(RefreshToken refreshToken, string? replacedByHash = null, CancellationToken cancellationToken = default);
    Task RevokeAllUserRefreshTokensAsync(Guid userId, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
