using Farmora.Application.DTO.Common;
using Farmora.Application.DTO.Verification;
using Farmora.Application.Interfaces.Services;
using Farmora.Domain.Entities;
using Farmora.Domain.Enums;
using Farmora.Infastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Farmora.Infastructure.Services;

public sealed class VerificationService(FarmoraDbContext db) : IVerificationService
{
    public async Task<VerificationResponse> SubmitAsync(SubmitVerificationDto dto, CancellationToken cancellationToken)
    {
        var farmer = await ResolveFarmerAsync(dto.FarmerId, cancellationToken)
            ?? throw new KeyNotFoundException("Farmer not found.");
        if (dto.Documents.Count == 0)
            throw new ArgumentException("At least one verification document is required.");

        var verification = new FarmerVerification(farmer.Id);
        db.Set<FarmerVerification>().Add(verification);
        await db.SaveChangesAsync(cancellationToken);

        var docs = dto.Documents.Select(d =>
        {
            if (string.IsNullOrWhiteSpace(d.StorageKey))
                throw new ArgumentException("Document storage key is required.");
            if (string.IsNullOrWhiteSpace(d.Url))
                throw new ArgumentException("Document url is required.");
            return new VerificationDocument(verification.Id, d.DocumentType, d.StorageKey.Trim(), d.Url.Trim());
        }).ToList();

        foreach (var doc in docs)
            verification.AddDocument(doc);
        db.Set<VerificationDocument>().AddRange(docs);

        farmer.SetVerification(FarmerVerificationStatus.Pending);
        await db.SaveChangesAsync(cancellationToken);
        return Map(verification, docs);
    }

    public async Task<VerificationResponse?> GetMyStatusAsync(Guid farmerId, CancellationToken cancellationToken)
    {
        var farmer = await ResolveFarmerAsync(farmerId, cancellationToken);
        if (farmer is null)
            return null;
        var verification = await db.Set<FarmerVerification>().AsNoTracking()
            .Where(v => v.FarmerId == farmer.Id)
            .OrderByDescending(v => v.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);
        if (verification is null)
            return null;
        var docs = await db.Set<VerificationDocument>().AsNoTracking()
            .Where(d => d.VerificationId == verification.Id)
            .ToListAsync(cancellationToken);
        return Map(verification, docs);
    }

    public async Task<PagedResult<VerificationResponse>> GetQueueAsync(string? status, int page, int pageSize, CancellationToken cancellationToken)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var query = db.Set<FarmerVerification>().AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(status))
        {
            if (!Enum.TryParse<FarmerVerificationStatus>(status.Trim(), true, out var parsed))
                throw new ArgumentException($"Unknown verification status '{status}'.");
            query = query.Where(v => v.Status == parsed);
        }
        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderBy(v => v.CreatedAtUtc)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync(cancellationToken);
        var ids = items.Select(v => v.Id).ToList();
        var docsByVerification = await db.Set<VerificationDocument>().AsNoTracking()
            .Where(d => ids.Contains(d.VerificationId))
            .GroupBy(d => d.VerificationId)
            .ToDictionaryAsync(g => g.Key, g => (IReadOnlyList<VerificationDocument>)g.ToList(), cancellationToken);

        var responses = items.Select(v => Map(v,
            docsByVerification.TryGetValue(v.Id, out var docs) ? docs : [])).ToList();
        return new PagedResult<VerificationResponse> { Items = responses, Page = page, PageSize = pageSize, TotalCount = total };
    }

    public async Task<VerificationResponse?> DecideAsync(Guid verificationId, Guid adminId, DecideVerificationDto dto, CancellationToken cancellationToken)
    {
        var verification = await db.Set<FarmerVerification>()
            .SingleOrDefaultAsync(v => v.Id == verificationId, cancellationToken);
        if (verification is null)
            return null;

        verification.Decide(dto.Status, adminId, dto.DecisionNote?.Trim(), dto.ExpiresAtUtc);

        var farmer = await db.Set<Farmer>().SingleOrDefaultAsync(f => f.Id == verification.FarmerId, cancellationToken);
        farmer?.SetVerification(dto.Status);
        farmer?.SetOperationalStatus(dto.Status == FarmerVerificationStatus.Approved
            ? FarmerOperationalStatus.Active
            : FarmerOperationalStatus.Inactive);

        await db.SaveChangesAsync(cancellationToken);
        var docs = await db.Set<VerificationDocument>().AsNoTracking()
            .Where(d => d.VerificationId == verification.Id)
            .ToListAsync(cancellationToken);
        return Map(verification, docs);
    }

    private async Task<Farmer?> ResolveFarmerAsync(Guid idOrUserId, CancellationToken ct) =>
        await db.Set<Farmer>().SingleOrDefaultAsync(f => f.Id == idOrUserId, ct)
        ?? await db.Set<Farmer>().SingleOrDefaultAsync(f => f.UserId == idOrUserId, ct);

    private static VerificationResponse Map(FarmerVerification v, IReadOnlyList<VerificationDocument> docs) => new()
    {
        Id = v.Id,
        FarmerId = v.FarmerId,
        Status = v.Status.ToString(),
        DecisionNote = v.DecisionNote,
        DecidedBy = v.DecidedBy,
        DecidedAtUtc = v.DecidedAtUtc,
        ExpiresAtUtc = v.ExpiresAtUtc,
        CreatedAtUtc = v.CreatedAtUtc,
        Documents = docs.Select(d => new VerificationDocumentDto
        {
            Id = d.Id,
            DocumentType = d.DocumentType.ToString(),
            StorageKey = d.StorageKey,
            Url = d.Url,
        }).ToList(),
    };
}
