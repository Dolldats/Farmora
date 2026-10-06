namespace Farmora.Application.Users;

public sealed record CreateUserRequest(string Email, string? Phone, string Password);
public sealed record UpdateUserRequest(string? Phone);
public sealed record UserResponse(Guid Id, string Email, string? Phone, string Status, DateTime CreatedAtUtc);
