using Farmora.Application.DTO.Address;

namespace Farmora.Application.Interfaces.Services;

public interface IAddressService
{
    Task<IReadOnlyList<AddressResponse>> ListAsync(Guid customerId, CancellationToken cancellationToken);
    Task<AddressResponse?> GetAsync(Guid customerId, Guid addressId, CancellationToken cancellationToken);
    Task<AddressResponse> CreateAsync(Guid customerId, CreateAddressDto dto, CancellationToken cancellationToken);
    Task<AddressResponse?> UpdateAsync(Guid customerId, Guid addressId, UpdateAddressDto dto, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid customerId, Guid addressId, CancellationToken cancellationToken);
    Task<bool> SetDefaultAsync(Guid customerId, Guid addressId, CancellationToken cancellationToken);
}
