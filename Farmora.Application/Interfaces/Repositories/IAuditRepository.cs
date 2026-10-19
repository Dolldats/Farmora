using Farmora.Application.Common;
using Farmora.Domain.Entities;

namespace Farmora.Application.Interfaces.Repositories;

public interface IAuditRepository
{
    Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default);
    Task<PagedResult<AuditLog>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);
}
