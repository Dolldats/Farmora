using Farmora.Application.DTO.Common;
using Farmora.Application.DTO.Reviews;
using Farmora.Application.Interfaces.Services;
using Farmora.Domain.Entities;
using Farmora.Domain.Enums;
using Farmora.Infastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Farmora.Infastructure.Services;

public sealed class ReviewService(FarmoraDbContext db) : IReviewService
{
    public async Task<ReviewResponse> CreateAsync(Guid customerId, CreateReviewDto dto, CancellationToken cancellationToken)
    {
        var customer = await ResolveCustomerAsync(customerId, cancellationToken)
            ?? throw new KeyNotFoundException("Customer not found.");
        if (dto.Rating < 1 || dto.Rating > 5)
            throw new ArgumentOutOfRangeException(nameof(dto.Rating), "Rating must be between 1 and 5.");

        if (await db.Set<Review>().AnyAsync(r => r.OrderItemId == dto.OrderItemId, cancellationToken))
            throw new InvalidOperationException("This order item has already been reviewed.");

        var orderItem = await db.Set<OrderItem>().SingleOrDefaultAsync(i => i.Id == dto.OrderItemId, cancellationToken)
            ?? throw new KeyNotFoundException("Order item not found.");
        var order = await db.Set<Order>().SingleOrDefaultAsync(o => o.Id == orderItem.OrderId, cancellationToken)
            ?? throw new KeyNotFoundException("Order not found.");
        if (order.CustomerId != customer.Id)
            throw new UnauthorizedAccessException("Only the purchasing customer can review this item.");

        var review = new Review(dto.OrderItemId, customer.Id, orderItem.ProductId, dto.Rating, dto.Comment);
        db.Set<Review>().Add(review);
        await db.SaveChangesAsync(cancellationToken);
        return Map(review);
    }

    public async Task<IReadOnlyList<ReviewResponse>> GetMyReviewsAsync(Guid customerId, CancellationToken cancellationToken)
    {
        var customer = await ResolveCustomerAsync(customerId, cancellationToken);
        if (customer is null)
            return [];
        return await db.Set<Review>().AsNoTracking()
            .Where(r => r.CustomerId == customer.Id)
            .OrderByDescending(r => r.CreatedAtUtc)
            .Select(r => new ReviewResponse
            {
                Id = r.Id,
                OrderItemId = r.OrderItemId,
                CustomerId = r.CustomerId,
                ProductId = r.ProductId,
                Rating = r.Rating,
                Comment = r.Comment,
                Status = r.Status.ToString(),
                CreatedAtUtc = r.CreatedAtUtc,
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<PagedResult<ReviewResponse>> GetByProductAsync(Guid productId, int page, int pageSize, CancellationToken cancellationToken)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var query = db.Set<Review>().AsNoTracking()
            .Where(r => r.ProductId == productId && r.Status == ReviewStatus.Approved);
        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(r => r.CreatedAtUtc)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(r => new ReviewResponse
            {
                Id = r.Id,
                OrderItemId = r.OrderItemId,
                CustomerId = r.CustomerId,
                ProductId = r.ProductId,
                Rating = r.Rating,
                Comment = r.Comment,
                Status = r.Status.ToString(),
                CreatedAtUtc = r.CreatedAtUtc,
            })
            .ToListAsync(cancellationToken);
        return new PagedResult<ReviewResponse> { Items = items, Page = page, PageSize = pageSize, TotalCount = total };
    }

    public async Task<ReviewResponse?> ModerateAsync(Guid reviewId, ModerateReviewDto dto, CancellationToken cancellationToken)
    {
        var review = await db.Set<Review>().SingleOrDefaultAsync(r => r.Id == reviewId, cancellationToken);
        if (review is null)
            return null;
        review.Moderate(dto.Status);
        await db.SaveChangesAsync(cancellationToken);
        return Map(review);
    }

    private async Task<Customer?> ResolveCustomerAsync(Guid idOrUserId, CancellationToken ct) =>
        await db.Set<Customer>().SingleOrDefaultAsync(c => c.Id == idOrUserId, ct)
        ?? await db.Set<Customer>().SingleOrDefaultAsync(c => c.UserId == idOrUserId, ct);

    private static ReviewResponse Map(Review r) => new()
    {
        Id = r.Id,
        OrderItemId = r.OrderItemId,
        CustomerId = r.CustomerId,
        ProductId = r.ProductId,
        Rating = r.Rating,
        Comment = r.Comment,
        Status = r.Status.ToString(),
        CreatedAtUtc = r.CreatedAtUtc,
    };
}
