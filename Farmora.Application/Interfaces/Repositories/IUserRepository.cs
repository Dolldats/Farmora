using Farmora.Domain.Entities;

namespace Farmora.Application.Interfaces.Repositories;

public interface IUserRepository
{
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken);
    Task<User?> GetAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken);
    Task AddAsync(User user, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
