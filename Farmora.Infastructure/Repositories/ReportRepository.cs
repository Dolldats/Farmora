using Farmora.Application.Interfaces.Repositories;
using Farmora.Domain.Entities;
using Farmora.Domain.Enums;
using Farmora.Infastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Farmora.Infastructure.Repositories;

public sealed class ReportRepository(FarmoraDbContext db) : IReportRepository
{
    public Task<Report?> GetAsync(Guid id, CancellationToken cancellationToken) =>
        db.Set<Report>().SingleOrDefaultAsync(r => r.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Report>> GetByReporterAsync(Guid reporterId, CancellationToken cancellationToken) =>
        await db.Set<Report>().AsNoTracking()
            .Where(r => r.ReporterId == reporterId)
            .OrderByDescending(r => r.CreatedAtUtc)
            .ToListAsync(cancellationToken);

    public async Task<(IReadOnlyList<Report> Items, int Total)> ListPagedAsync(
        ReportStatus? status, int page, int pageSize, CancellationToken cancellationToken)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var query = db.Set<Report>().AsNoTracking().AsQueryable();
        if (status.HasValue)
            query = query.Where(r => r.Status == status.Value);
        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(r => r.CreatedAtUtc)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return (items, total);
    }

    public Task<int> CountByStatusAsync(ReportStatus status, CancellationToken cancellationToken) =>
        db.Set<Report>().CountAsync(r => r.Status == status, cancellationToken);

    public Task AddAsync(Report report, CancellationToken cancellationToken) =>
        db.Set<Report>().AddAsync(report, cancellationToken).AsTask();

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        db.SaveChangesAsync(cancellationToken);
}
