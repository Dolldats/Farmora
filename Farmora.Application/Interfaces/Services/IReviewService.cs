using Farmora.Application.DTO.Common;
using Farmora.Application.DTO.Reviews;

namespace Farmora.Application.Interfaces.Services;

public interface IReviewService
{
    Task<ReviewResponse> CreateAsync(Guid customerId, CreateReviewDto dto, CancellationToken cancellationToken);
    Task<IReadOnlyList<ReviewResponse>> GetMyReviewsAsync(Guid customerId, CancellationToken cancellationToken);
    Task<PagedResult<ReviewResponse>> GetByProductAsync(Guid productId, int page, int pageSize, CancellationToken cancellationToken);
    Task<ReviewResponse?> ModerateAsync(Guid reviewId, ModerateReviewDto dto, CancellationToken cancellationToken);
}
