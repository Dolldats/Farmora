using Farmora.Application.DTO.Promotions;

namespace Farmora.Application.Interfaces.Services;

public interface IPromotionService
{
    Task<PromotionResponse> CreateAsync(CreatePromotionDto dto, CancellationToken cancellationToken);
    Task<PromotionResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<PromotionResponse?> GetByCodeAsync(string code, CancellationToken cancellationToken);
    Task<IReadOnlyList<PromotionResponse>> ListAsync(CancellationToken cancellationToken);
    Task<bool> DisableAsync(Guid id, CancellationToken cancellationToken);
    Task<decimal> ValidateAsync(ApplyPromoDto dto, CancellationToken cancellationToken);
}
