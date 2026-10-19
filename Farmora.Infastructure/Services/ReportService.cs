using Farmora.Application.DTO.Common;
using Farmora.Application.DTO.Reports;
using Farmora.Application.Interfaces.Services;
using Farmora.Domain.Entities;
using Farmora.Domain.Enums;
using Farmora.Infastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Farmora.Infastructure.Services;

public sealed class ReportService(FarmoraDbContext db) : IReportService
{
    public async Task<ReportResponse> CreateAsync(Guid reporterId, CreateReportDto dto, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dto.TargetType))
            throw new ArgumentException("Target type is required.");
        if (dto.TargetId == Guid.Empty)
            throw new ArgumentException("Target id is required.");
        if (string.IsNullOrWhiteSpace(dto.Reason))
            throw new ArgumentException("Reason is required.");

        var report = new Report(reporterId, dto.TargetType.Trim(), dto.TargetId, dto.Reason.Trim());
        if (!string.IsNullOrWhiteSpace(dto.EvidenceUrl))
            report.SetEvidence(dto.EvidenceUrl.Trim());
        db.Set<Report>().Add(report);
        await db.SaveChangesAsync(cancellationToken);
        return Map(report);
    }

    public async Task<IReadOnlyList<ReportResponse>> GetMyReportsAsync(Guid reporterId, CancellationToken cancellationToken) =>
        await db.Set<Report>().AsNoTracking()
            .Where(r => r.ReporterId == reporterId)
            .OrderByDescending(r => r.CreatedAtUtc)
            .Select(r => new ReportResponse
            {
                Id = r.Id,
                ReporterId = r.ReporterId,
                TargetType = r.TargetType,
                TargetId = r.TargetId,
                Reason = r.Reason,
                EvidenceUrl = r.EvidenceUrl,
                Status = r.Status.ToString(),
                AssignedTo = r.AssignedTo,
                Resolution = r.Resolution,
                CreatedAtUtc = r.CreatedAtUtc,
            })
            .ToListAsync(cancellationToken);

    public async Task<PagedResult<ReportResponse>> ListAsync(string? status, int page, int pageSize, CancellationToken cancellationToken)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var query = db.Set<Report>().AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(status))
        {
            if (!Enum.TryParse<ReportStatus>(status.Trim(), true, out var parsed))
                throw new ArgumentException($"Unknown report status '{status}'.");
            query = query.Where(r => r.Status == parsed);
        }
        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(r => r.CreatedAtUtc)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(r => new ReportResponse
            {
                Id = r.Id,
                ReporterId = r.ReporterId,
                TargetType = r.TargetType,
                TargetId = r.TargetId,
                Reason = r.Reason,
                EvidenceUrl = r.EvidenceUrl,
                Status = r.Status.ToString(),
                AssignedTo = r.AssignedTo,
                Resolution = r.Resolution,
                CreatedAtUtc = r.CreatedAtUtc,
            })
            .ToListAsync(cancellationToken);
        return new PagedResult<ReportResponse> { Items = items, Page = page, PageSize = pageSize, TotalCount = total };
    }

    public async Task<ReportResponse?> AssignAsync(Guid reportId, AssignReportDto dto, CancellationToken cancellationToken)
    {
        var report = await db.Set<Report>().SingleOrDefaultAsync(r => r.Id == reportId, cancellationToken);
        if (report is null)
            return null;
        if (dto.AdminId == Guid.Empty)
            throw new ArgumentException("Admin id is required.");
        report.Assign(dto.AdminId);
        await db.SaveChangesAsync(cancellationToken);
        return Map(report);
    }

    public async Task<ReportResponse?> ResolveAsync(Guid reportId, ResolveReportDto dto, CancellationToken cancellationToken)
    {
        var report = await db.Set<Report>().SingleOrDefaultAsync(r => r.Id == reportId, cancellationToken);
        if (report is null)
            return null;
        if (string.IsNullOrWhiteSpace(dto.Resolution))
            throw new ArgumentException("Resolution is required.");
        report.Resolve(dto.Resolution.Trim(), dto.Dismissed);
        await db.SaveChangesAsync(cancellationToken);
        return Map(report);
    }

    private static ReportResponse Map(Report r) => new()
    {
        Id = r.Id,
        ReporterId = r.ReporterId,
        TargetType = r.TargetType,
        TargetId = r.TargetId,
        Reason = r.Reason,
        EvidenceUrl = r.EvidenceUrl,
        Status = r.Status.ToString(),
        AssignedTo = r.AssignedTo,
        Resolution = r.Resolution,
        CreatedAtUtc = r.CreatedAtUtc,
    };
}
