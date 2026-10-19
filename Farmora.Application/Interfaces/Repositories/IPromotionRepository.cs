using Farmora.Domain.Entities;

namespace Farmora.Application.Interfaces.Repositories;

public interface IPromotionRepository
{
    Task<Promotion?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Promotion?> GetByCodeAsync(string code, CancellationToken cancellationToken);
    Task<IReadOnlyList<Promotion>> ListAsync(CancellationToken cancellationToken);
    Task AddAsync(Promotion promotion, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
