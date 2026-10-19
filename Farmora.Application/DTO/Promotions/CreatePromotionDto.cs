using Farmora.Domain.Enums;

namespace Farmora.Application.DTO.Promotions;

public sealed class CreatePromotionDto
{
    public string Code { get; init; } = string.Empty;
    public PromotionType Type { get; init; }
    public decimal Value { get; init; }
    public DateTime StartsAtUtc { get; init; }
    public DateTime EndsAtUtc { get; init; }
    public int? MaxUses { get; init; }
    public decimal? MinOrderAmount { get; init; }
}
