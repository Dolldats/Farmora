using Farmora.Application.DTO.User;

namespace Farmora.Application.Interfaces.Services;

public interface IUserService
{
    Task<UserResponse> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken);
    Task<UserResponse?> GetAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<UserResponse>> GetAllAsync(CancellationToken cancellationToken);
    Task<UserResponse?> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken);
    Task<bool> DisableAsync(Guid id, CancellationToken cancellationToken);
}
