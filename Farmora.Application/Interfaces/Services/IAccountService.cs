using Farmora.Application.DTO.Account;

namespace Farmora.Application.Interfaces.Services;

public interface IAccountService
{
    Task<MyProfileResponse?> GetMeAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<MyProfileResponse?> UpdateMeAsync(Guid userId, UpdateMyProfileDto dto, CancellationToken cancellationToken = default);
    Task<CustomerResponse?> GetMyCustomerAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<CustomerResponse> UpsertMyCustomerAsync(Guid userId, UpdateCustomerDto dto, CancellationToken cancellationToken = default);
    Task<FarmerResponse?> GetMyFarmerAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<FarmerResponse> UpsertMyFarmerAsync(Guid userId, UpdateFarmerDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteMyCustomerAsync(Guid userId, CancellationToken cancellationToken = default);
}
