using Farmora.Domain.Entities;
using Farmora.Application.DTO.Analytics;

namespace Farmora.Application.Interfaces.Repositories;

public interface IAnalyticsRepository
{
    Task AddAsync(AnalyticsEvent evt, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}

public interface IDashboardRepository
{
    Task<CustomerDashboardDto> GetCustomerDashboardAsync(Guid customerId, CancellationToken cancellationToken);
    Task<FarmerDashboardDto> GetFarmerDashboardAsync(Guid farmerId, CancellationToken cancellationToken);
    Task<AdminDashboardDto> GetAdminDashboardAsync(CancellationToken cancellationToken);
}
