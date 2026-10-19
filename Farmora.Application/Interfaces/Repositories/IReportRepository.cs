using Farmora.Domain.Entities;
using Farmora.Domain.Enums;

namespace Farmora.Application.Interfaces.Repositories;

public interface IReportRepository
{
    Task<Report?> GetAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<Report>> GetByReporterAsync(Guid reporterId, CancellationToken cancellationToken);
    Task<(IReadOnlyList<Report> Items, int Total)> ListPagedAsync(ReportStatus? status, int page, int pageSize, CancellationToken cancellationToken);
    Task<int> CountByStatusAsync(ReportStatus status, CancellationToken cancellationToken);
    Task AddAsync(Report report, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
