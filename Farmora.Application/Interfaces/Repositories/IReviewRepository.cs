using Farmora.Domain.Entities;
using Farmora.Domain.Enums;

namespace Farmora.Application.Interfaces.Repositories;

public interface IReviewRepository
{
    Task<bool> ExistsForOrderItemAsync(Guid orderItemId, CancellationToken cancellationToken);
    Task<Review?> GetAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<Review>> GetByCustomerAsync(Guid customerId, CancellationToken cancellationToken);
    Task<(IReadOnlyList<Review> Items, int Total)> GetByProductPagedAsync(Guid productId, ReviewStatus? approvedOnly, int page, int pageSize, CancellationToken cancellationToken);
    Task<double> GetAverageRatingAsync(Guid productId, CancellationToken cancellationToken);
    Task AddAsync(Review review, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
