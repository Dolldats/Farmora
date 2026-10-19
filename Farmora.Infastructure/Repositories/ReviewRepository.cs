using Farmora.Application.Interfaces.Repositories;
using Farmora.Domain.Entities;
using Farmora.Domain.Enums;
using Farmora.Infastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Farmora.Infastructure.Repositories;

public sealed class ReviewRepository(FarmoraDbContext db) : IReviewRepository
{
    public Task<bool> ExistsForOrderItemAsync(Guid orderItemId, CancellationToken cancellationToken) =>
        db.Set<Review>().AnyAsync(r => r.OrderItemId == orderItemId, cancellationToken);

    public Task<Review?> GetAsync(Guid id, CancellationToken cancellationToken) =>
        db.Set<Review>().SingleOrDefaultAsync(r => r.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Review>> GetByCustomerAsync(Guid customerId, CancellationToken cancellationToken) =>
        await db.Set<Review>().AsNoTracking()
            .Where(r => r.CustomerId == customerId)
            .OrderByDescending(r => r.CreatedAtUtc)
            .ToListAsync(cancellationToken);

    public async Task<(IReadOnlyList<Review> Items, int Total)> GetByProductPagedAsync(
        Guid productId, ReviewStatus? approvedOnly, int page, int pageSize, CancellationToken cancellationToken)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var query = db.Set<Review>().AsNoTracking().Where(r => r.ProductId == productId);
        if (approvedOnly.HasValue)
            query = query.Where(r => r.Status == approvedOnly.Value);
        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(r => r.CreatedAtUtc)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return (items, total);
    }

    public async Task<double> GetAverageRatingAsync(Guid productId, CancellationToken cancellationToken)
    {
        var avg = await db.Set<Review>().AsNoTracking()
            .Where(r => r.ProductId == productId && r.Status == ReviewStatus.Approved)
            .Select(r => (double?)r.Rating)
            .AverageAsync(cancellationToken);
        return avg ?? 0;
    }

    public Task AddAsync(Review review, CancellationToken cancellationToken) =>
        db.Set<Review>().AddAsync(review, cancellationToken).AsTask();

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        db.SaveChangesAsync(cancellationToken);
}
