using Farmora.Application.DTO.Analytics;

namespace Farmora.Application.Interfaces.Services;

public interface IAnalyticsService
{
    Task TrackAsync(AnalyticsEventDto dto, CancellationToken cancellationToken);
    Task<FarmerDashboardDto> GetFarmerDashboardAsync(Guid farmerId, CancellationToken cancellationToken);
    Task<AdminDashboardDto> GetAdminDashboardAsync(CancellationToken cancellationToken);
}

public interface IDashboardService
{
    Task<CustomerDashboardDto> GetCustomerDashboardAsync(Guid customerId, CancellationToken cancellationToken);
    Task<FarmerDashboardDto> GetFarmerDashboardAsync(Guid farmerId, CancellationToken cancellationToken);
    Task<AdminDashboardDto> GetAdminDashboardAsync(CancellationToken cancellationToken);
}
