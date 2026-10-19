namespace Farmora.Application.DTO.Analytics;

public sealed class AnalyticsEventDto
{
    public string EventType { get; set; } = string.Empty;
    public Guid? ActorId { get; set; }
    public Guid? ResourceId { get; set; }
    public string? Metadata { get; set; }
}

public sealed class CustomerDashboardDto
{
    public int TotalOrders { get; init; }
    public decimal TotalSpent { get; init; }
    public int FavoriteCount { get; init; }
    public int FollowedFarmersCount { get; init; }
    public int UnreadNotifications { get; init; }
}

public sealed class FarmerDashboardDto
{
    public Guid FarmerId { get; init; }
    public decimal TotalSales { get; init; }
    public int TotalOrders { get; init; }
    public int ActiveProducts { get; init; }
    public int LowStockProducts { get; init; }
    public int OutOfStockProducts { get; init; }
    public int PendingOrders { get; init; }
    public double AverageRating { get; init; }
    public int FollowerCount { get; init; }
}

public sealed class AdminDashboardDto
{
    public int TotalUsers { get; init; }
    public int TotalFarmers { get; init; }
    public int TotalCustomers { get; init; }
    public int TotalProducts { get; init; }
    public int TotalOrders { get; init; }
    public decimal TotalRevenue { get; init; }
    public int OpenReports { get; init; }
    public int PendingVerifications { get; init; }
    public int VerificationQueueCount { get; init; }
}
