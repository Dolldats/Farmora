using Farmora.Application.Interfaces.Repositories;
using Farmora.Domain.Entities;
using Farmora.Infastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Farmora.Infastructure.Repositories;

public sealed class PromotionRepository(FarmoraDbContext db) : IPromotionRepository
{
    public Task<Promotion?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        db.Set<Promotion>()
            .Include(p => p.Redemptions)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public Task<Promotion?> GetByCodeAsync(string code, CancellationToken cancellationToken)
    {
        var normalized = code.Trim().ToUpperInvariant();
        return db.Set<Promotion>()
            .Include(p => p.Redemptions)
            .FirstOrDefaultAsync(p => p.Code == normalized, cancellationToken);
    }

    public async Task<IReadOnlyList<Promotion>> ListAsync(CancellationToken cancellationToken) =>
        await db.Set<Promotion>()
            .AsNoTracking()
            .OrderBy(p => p.Code)
            .ToListAsync(cancellationToken);

    public Task AddAsync(Promotion promotion, CancellationToken cancellationToken) =>
        db.Set<Promotion>().AddAsync(promotion, cancellationToken).AsTask();

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        db.SaveChangesAsync(cancellationToken);
}
