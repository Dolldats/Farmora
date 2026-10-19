using Farmora.Application.Common;
using Farmora.Application.Interfaces.Repositories;
using Farmora.Domain.Entities;
using Farmora.Infastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Farmora.Infastructure.Repositories;

public sealed class AuditRepository(FarmoraDbContext db) : IAuditRepository
{
    public Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default)
    {
        return db.AuditLogs.AddAsync(auditLog, cancellationToken).AsTask();
    }

    public async Task<PagedResult<AuditLog>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 20 : pageSize > 100 ? 100 : pageSize;

        var total = await db.AuditLogs.CountAsync(cancellationToken);
        var items = await db.AuditLogs
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return PagedResult<AuditLog>.Create(items, total, page, pageSize);
    }
}
