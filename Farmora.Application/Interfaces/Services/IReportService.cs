using Farmora.Application.DTO.Common;
using Farmora.Application.DTO.Reports;

namespace Farmora.Application.Interfaces.Services;

public interface IReportService
{
    Task<ReportResponse> CreateAsync(Guid reporterId, CreateReportDto dto, CancellationToken cancellationToken);
    Task<IReadOnlyList<ReportResponse>> GetMyReportsAsync(Guid reporterId, CancellationToken cancellationToken);
    Task<PagedResult<ReportResponse>> ListAsync(string? status, int page, int pageSize, CancellationToken cancellationToken);
    Task<ReportResponse?> AssignAsync(Guid reportId, AssignReportDto dto, CancellationToken cancellationToken);
    Task<ReportResponse?> ResolveAsync(Guid reportId, ResolveReportDto dto, CancellationToken cancellationToken);
}
