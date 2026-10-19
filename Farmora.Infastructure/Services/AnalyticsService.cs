using Farmora.Application.DTO.Analytics;
using Farmora.Application.Interfaces.Repositories;
using Farmora.Application.Interfaces.Services;
using Farmora.Domain.Entities;

namespace Farmora.Infastructure.Services;

public sealed class AnalyticsService(
    IAnalyticsRepository analytics,
    IDashboardRepository dashboards) : IAnalyticsService, IDashboardService
{
    public async Task TrackAsync(AnalyticsEventDto dto, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dto.EventType))
            throw new ArgumentException("Event type is required.");
        await analytics.AddAsync(
            new AnalyticsEvent(dto.EventType.Trim(), dto.ActorId, dto.ResourceId, dto.Metadata),
            cancellationToken);
        await analytics.SaveChangesAsync(cancellationToken);
    }

    public Task<FarmerDashboardDto> GetFarmerDashboardAsync(Guid farmerId, CancellationToken cancellationToken) =>
        dashboards.GetFarmerDashboardAsync(farmerId, cancellationToken);

    public Task<AdminDashboardDto> GetAdminDashboardAsync(CancellationToken cancellationToken) =>
        dashboards.GetAdminDashboardAsync(cancellationToken);

    public Task<CustomerDashboardDto> GetCustomerDashboardAsync(Guid customerId, CancellationToken cancellationToken) =>
        dashboards.GetCustomerDashboardAsync(customerId, cancellationToken);
}
