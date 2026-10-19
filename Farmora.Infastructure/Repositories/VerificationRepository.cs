using Farmora.Application.Interfaces.Repositories;
using Farmora.Domain.Entities;
using Farmora.Domain.Enums;
using Farmora.Infastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Farmora.Infastructure.Repositories;

public sealed class VerificationRepository(FarmoraDbContext db) : IVerificationRepository
{
    public Task<FarmerVerification?> GetAsync(Guid id, CancellationToken cancellationToken) =>
        db.Set<FarmerVerification>().SingleOrDefaultAsync(v => v.Id == id, cancellationToken);

    public Task<FarmerVerification?> GetByFarmerAsync(Guid farmerId, CancellationToken cancellationToken) =>
        db.Set<FarmerVerification>().AsNoTracking()
            .Where(v => v.FarmerId == farmerId)
            .OrderByDescending(v => v.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<(IReadOnlyList<FarmerVerification> Items, int Total)> GetQueuePagedAsync(
        FarmerVerificationStatus? status, int page, int pageSize, CancellationToken cancellationToken)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var query = db.Set<FarmerVerification>().AsNoTracking().AsQueryable();
        if (status.HasValue)
            query = query.Where(v => v.Status == status.Value);
        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderBy(v => v.CreatedAtUtc)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return (items, total);
    }

    public Task<int> CountByStatusAsync(FarmerVerificationStatus status, CancellationToken cancellationToken) =>
        db.Set<FarmerVerification>().CountAsync(v => v.Status == status, cancellationToken);

    public Task AddAsync(FarmerVerification verification, CancellationToken cancellationToken) =>
        db.Set<FarmerVerification>().AddAsync(verification, cancellationToken).AsTask();

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        db.SaveChangesAsync(cancellationToken);
}
