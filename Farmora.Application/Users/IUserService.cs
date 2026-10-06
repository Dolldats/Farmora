namespace Farmora.Application.Users;

public interface IUserService
{
    Task<UserResponse> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken);
    Task<UserResponse?> GetAsync(Guid id, CancellationToken cancellationToken);
    Task<UserResponse?> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken);
    Task<bool> DisableAsync(Guid id, CancellationToken cancellationToken);
}
