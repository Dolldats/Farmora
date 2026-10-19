using Farmora.Domain.Entities;

namespace Farmora.Application.Interfaces.Repositories;

public interface IFarmerRepository
{
    Task<Farmer?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(Farmer farmer, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
