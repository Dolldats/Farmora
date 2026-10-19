using Farmora.Application.DTO.Analytics;
using Farmora.Application.Interfaces.Repositories;
using Farmora.Domain.Entities;
using Farmora.Domain.Enums;
using Farmora.Infastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Farmora.Infastructure.Repositories;

public sealed class AnalyticsRepository(FarmoraDbContext db) : IAnalyticsRepository, IDashboardRepository
{
    private static readonly OrderStatus[] PaidStatuses =
        [OrderStatus.Paid, OrderStatus.Processing, OrderStatus.Shipped, OrderStatus.Delivered];

    private static readonly OrderStatus[] PendingStatuses =
        [OrderStatus.Pending, OrderStatus.PaymentPending, OrderStatus.Processing];

    private const decimal LowStockThreshold = 10m;

    public Task AddAsync(AnalyticsEvent evt, CancellationToken cancellationToken) =>
        db.Set<AnalyticsEvent>().AddAsync(evt, cancellationToken).AsTask();

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        db.SaveChangesAsync(cancellationToken);

    public async Task<CustomerDashboardDto> GetCustomerDashboardAsync(Guid customerId, CancellationToken cancellationToken)
    {
        var customer = await ResolveCustomerAsync(customerId, cancellationToken);
        if (customer is null)
            return new CustomerDashboardDto();

        var ordersQuery = db.Set<Order>().AsNoTracking().Where(o => o.CustomerId == customer.Id);
        var totalOrders = await ordersQuery.CountAsync(cancellationToken);
        var totalSpent = await ordersQuery
            .Where(o => PaidStatuses.Contains(o.Status))
            .SumAsync(o => (decimal?)o.Total, cancellationToken) ?? 0m;

        var favoriteCount = await db.Set<ProductFavorite>().CountAsync(
            f => f.CustomerId == customer.Id, cancellationToken);
        var followedFarmersCount = await db.Set<FarmerFollow>().CountAsync(
            f => f.CustomerId == customer.Id, cancellationToken);
        var unreadNotifications = await db.Set<Notification>().CountAsync(
            n => n.RecipientId == customer.UserId && !n.IsRead, cancellationToken);

        return new CustomerDashboardDto
        {
            TotalOrders = totalOrders,
            TotalSpent = totalSpent,
            FavoriteCount = favoriteCount,
            FollowedFarmersCount = followedFarmersCount,
            UnreadNotifications = unreadNotifications,
        };
    }

    public async Task<FarmerDashboardDto> GetFarmerDashboardAsync(Guid farmerId, CancellationToken cancellationToken)
    {
        var farmer = await ResolveFarmerAsync(farmerId, cancellationToken);
        if (farmer is null)
            return new FarmerDashboardDto { FarmerId = farmerId };

        var productIds = await db.Set<Product>().AsNoTracking()
            .Where(p => p.FarmerId == farmer.Id)
            .Select(p => p.Id)
            .ToListAsync(cancellationToken);

        var itemsQuery = db.Set<OrderItem>().AsNoTracking().Where(i => i.FarmerId == farmer.Id);
        var paidOrderIds = await itemsQuery
            .Where(i => PaidStatuses.Contains(i.Order.Status))
            .Select(i => i.OrderId)
            .Distinct()
            .ToListAsync(cancellationToken);
        var totalSales = await itemsQuery
            .Where(i => PaidStatuses.Contains(i.Order.Status))
            .SumAsync(i => (decimal?)i.LineTotal, cancellationToken) ?? 0m;
        var pendingOrders = await itemsQuery
            .Where(i => PendingStatuses.Contains(i.Order.Status))
            .Select(i => i.OrderId)
            .Distinct()
            .CountAsync(cancellationToken);

        var productsQuery = db.Set<Product>().AsNoTracking().Where(p => p.FarmerId == farmer.Id);
        var activeProducts = await productsQuery
            .CountAsync(p => p.Status == ProductStatus.Published && p.IsAvailable, cancellationToken);
        var lowStockProducts = await productsQuery
            .CountAsync(p => p.Stock > 0 && p.Stock < LowStockThreshold, cancellationToken);
        var outOfStockProducts = await productsQuery
            .CountAsync(p => p.Stock <= 0, cancellationToken);

        var averageRating = productIds.Count == 0 ? 0 :
            await db.Set<Review>().AsNoTracking()
                .Where(r => productIds.Contains(r.ProductId) && r.Status == ReviewStatus.Approved)
                .Select(r => (double?)r.Rating)
                .AverageAsync(cancellationToken) ?? 0;

        var followerCount = await db.Set<FarmerFollow>()
            .CountAsync(f => f.FarmerId == farmer.Id, cancellationToken);

        return new FarmerDashboardDto
        {
            FarmerId = farmer.Id,
            TotalSales = totalSales,
            TotalOrders = paidOrderIds.Count,
            ActiveProducts = activeProducts,
            LowStockProducts = lowStockProducts,
            OutOfStockProducts = outOfStockProducts,
            PendingOrders = pendingOrders,
            AverageRating = averageRating,
            FollowerCount = followerCount,
        };
    }

    public async Task<AdminDashboardDto> GetAdminDashboardAsync(CancellationToken cancellationToken)
    {
        var totalUsers = await db.Users.CountAsync(cancellationToken);
        var totalFarmers = await db.Set<Farmer>().CountAsync(cancellationToken);
        var totalCustomers = await db.Set<Customer>().CountAsync(cancellationToken);
        var totalProducts = await db.Set<Product>().CountAsync(cancellationToken);
        var totalOrders = await db.Set<Order>().CountAsync(cancellationToken);
        var totalRevenue = await db.Set<Order>().AsNoTracking()
            .Where(o => PaidStatuses.Contains(o.Status))
            .SumAsync(o => (decimal?)o.Total, cancellationToken) ?? 0m;
        var openReports = await db.Set<Report>()
            .CountAsync(r => r.Status == ReportStatus.Open, cancellationToken);
        var pendingVerifications = await db.Set<FarmerVerification>()
            .CountAsync(v => v.Status == FarmerVerificationStatus.Pending, cancellationToken);

        return new AdminDashboardDto
        {
            TotalUsers = totalUsers,
            TotalFarmers = totalFarmers,
            TotalCustomers = totalCustomers,
            TotalProducts = totalProducts,
            TotalOrders = totalOrders,
            TotalRevenue = totalRevenue,
            OpenReports = openReports,
            PendingVerifications = pendingVerifications,
            VerificationQueueCount = pendingVerifications,
        };
    }

    private async Task<Customer?> ResolveCustomerAsync(Guid idOrUserId, CancellationToken ct) =>
        await db.Set<Customer>().SingleOrDefaultAsync(c => c.Id == idOrUserId, ct)
        ?? await db.Set<Customer>().SingleOrDefaultAsync(c => c.UserId == idOrUserId, ct);

    private async Task<Farmer?> ResolveFarmerAsync(Guid idOrUserId, CancellationToken ct) =>
        await db.Set<Farmer>().SingleOrDefaultAsync(f => f.Id == idOrUserId, ct)
        ?? await db.Set<Farmer>().SingleOrDefaultAsync(f => f.UserId == idOrUserId, ct);
}
