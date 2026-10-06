using Farmora.Domain.Entities;
using Farmora.Application.DTO.User;
using Farmora.Application.Interfaces.Repositories;
using Farmora.Application.Interfaces.Services;
using Microsoft.AspNetCore.Identity;

namespace Farmora.Infastructure.Services;

public sealed class UserService(IUserRepository repository, IPasswordHasher<User> passwordHasher) : IUserService
{
    public async Task<UserResponse> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            throw new ArgumentException("Email is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ArgumentException("Password is required.");
        }

        var email = request.Email.Trim().ToLowerInvariant();
        if (await repository.EmailExistsAsync(email, cancellationToken))
        {
            throw new InvalidOperationException("A user with this email already exists.");
        }

        var user = new User(email, request.Phone, string.Empty);
        user.UpdateProfile(email, request.Phone, passwordHasher.HashPassword(user, request.Password));

        await repository.AddAsync(user, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return Map(user)!;
    }

    public async Task<UserResponse?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        return Map(await repository.GetAsync(id, cancellationToken));
    }

    public async Task<IReadOnlyList<UserResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        var users = await repository.GetAllAsync(cancellationToken);
        var responses = new List<UserResponse>();

        foreach (var user in users)
        {
            var response = Map(user);

            if (response is not null)
            {
                responses.Add(response);
            }
        }

        return responses;
    }

    public async Task<UserResponse?> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken)
    {
        var user = await repository.GetAsync(id, cancellationToken);
        if (user is null) return null;
        if (string.IsNullOrWhiteSpace(request.Email)) throw new ArgumentException("Email is required.");
        if (string.IsNullOrWhiteSpace(request.Password)) throw new ArgumentException("Password is required.");
        var email = request.Email.Trim().ToLowerInvariant();
        if (!string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase)
            && await repository.EmailExistsAsync(email, cancellationToken))
            throw new InvalidOperationException("A user with this email already exists.");
        user.UpdateProfile(email, request.Phone, passwordHasher.HashPassword(user, request.Password));
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

    private static UserResponse? Map(User? user)
    {
        if (user is null)
        {
            return null;
        }

        return new UserResponse
        {
            Id = user.Id,
            Email = user.Email,
            Phone = user.Phone,
            Status = user.Status.ToString(),
            CreatedAtUtc = user.CreatedAtUtc
        };
    }
}
