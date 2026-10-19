using Farmora.Domain.Entities;
using Farmora.Domain.Enums;

namespace Farmora.Application.Interfaces.Repositories;

public interface IVerificationRepository
{
    Task<FarmerVerification?> GetAsync(Guid id, CancellationToken cancellationToken);
    Task<FarmerVerification?> GetByFarmerAsync(Guid farmerId, CancellationToken cancellationToken);
    Task<(IReadOnlyList<FarmerVerification> Items, int Total)> GetQueuePagedAsync(FarmerVerificationStatus? status, int page, int pageSize, CancellationToken cancellationToken);
    Task<int> CountByStatusAsync(FarmerVerificationStatus status, CancellationToken cancellationToken);
    Task AddAsync(FarmerVerification verification, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
