using Farmora.Application.DTO.Promotions;
using Farmora.Application.Interfaces.Repositories;
using Farmora.Application.Interfaces.Services;
using Farmora.Domain.Entities;

namespace Farmora.Infastructure.Services;

public sealed class PromotionService(IPromotionRepository promotions) : IPromotionService
{
    public async Task<PromotionResponse> CreateAsync(CreatePromotionDto dto, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dto.Code))
            throw new ArgumentException("Promotion code is required.");
        if (dto.EndsAtUtc <= dto.StartsAtUtc)
            throw new ArgumentException("EndsAtUtc must be after StartsAtUtc.");

        var existing = await promotions.GetByCodeAsync(dto.Code, cancellationToken);
        if (existing is not null)
            throw new InvalidOperationException("A promotion with this code already exists.");

        var promo = new Promotion(dto.Code, dto.Type, dto.Value, dto.StartsAtUtc, dto.EndsAtUtc, dto.MaxUses);
        promo.SetMinOrder(dto.MinOrderAmount);
        await promotions.AddAsync(promo, cancellationToken);
        await promotions.SaveChangesAsync(cancellationToken);
        return Map(promo);
    }

    public async Task<PromotionResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        await promotions.GetByIdAsync(id, cancellationToken) is { } p ? Map(p) : null;

    public async Task<PromotionResponse?> GetByCodeAsync(string code, CancellationToken cancellationToken) =>
        await promotions.GetByCodeAsync(code, cancellationToken) is { } p ? Map(p) : null;

    public async Task<IReadOnlyList<PromotionResponse>> ListAsync(CancellationToken cancellationToken)
    {
        var list = await promotions.ListAsync(cancellationToken);
        return list.Select(Map).ToList();
    }

    public async Task<bool> DisableAsync(Guid id, CancellationToken cancellationToken)
    {
        var promo = await promotions.GetByIdAsync(id, cancellationToken);
        if (promo is null) return false;
        promo.Disable();
        await promotions.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<decimal> ValidateAsync(ApplyPromoDto dto, CancellationToken cancellationToken)
    {
        var promo = await promotions.GetByCodeAsync(dto.Code, cancellationToken)
            ?? throw new InvalidOperationException("Promotion code not found.");
        var now = DateTime.UtcNow;
        if (!promo.IsUsable(now))
            throw new InvalidOperationException("Promotion is not usable.");
        if (promo.MinOrderAmount is not null && dto.OrderSubtotal < promo.MinOrderAmount)
            throw new InvalidOperationException("Order subtotal does not meet the promotion minimum.");
        return promo.DiscountFor(dto.OrderSubtotal);
    }

    private static PromotionResponse Map(Promotion p) => new()
    {
        Id = p.Id,
        Code = p.Code,
        Type = p.Type,
        Value = p.Value,
        StartsAtUtc = p.StartsAtUtc,
        EndsAtUtc = p.EndsAtUtc,
        MaxUses = p.MaxUses,
        UseCount = p.UseCount,
        Status = p.Status,
        MinOrderAmount = p.MinOrderAmount,
    };
}
