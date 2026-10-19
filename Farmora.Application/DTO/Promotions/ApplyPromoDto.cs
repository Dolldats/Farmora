namespace Farmora.Application.DTO.Promotions;

public sealed class ApplyPromoDto
{
    public string Code { get; init; } = string.Empty;
    public decimal OrderSubtotal { get; init; }
}
