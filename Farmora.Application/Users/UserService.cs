using Farmora.Domain.Entities;

namespace Farmora.Application.Users;

public sealed class UserService(IUserRepository repository) : IUserService
{
    public async Task<UserResponse> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email)) throw new ArgumentException("Email is required.");
        if (string.IsNullOrWhiteSpace(request.Password)) throw new ArgumentException("Password is required.");

        var email = request.Email.Trim().ToLowerInvariant();
        if (await repository.EmailExistsAsync(email, cancellationToken))
            throw new InvalidOperationException("A user with this email already exists.");

        var user = new User(email, request.Phone, request.Password);
        await repository.AddAsync(user, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return Map(user)!;
    }

    public async Task<UserResponse?> GetAsync(Guid id, CancellationToken cancellationToken)
        => Map(await repository.GetAsync(id, cancellationToken));

    public async Task<UserResponse?> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken)
    {
        var user = await repository.GetAsync(id, cancellationToken);
        if (user is null) return null;
        user.UpdatePhone(request.Phone);
        await repository.SaveChangesAsync(cancellationToken);
        return Map(user);
    }

    public async Task<bool> DisableAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = await repository.GetAsync(id, cancellationToken);
        if (user is null) return false;
        user.Disable();
        await repository.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static UserResponse? Map(User? user) => user is null ? null : new(user.Id, user.Email, user.Phone, user.Status.ToString(), user.CreatedAtUtc);
}
