namespace Farmora.Application.DTO.Auth;

public sealed class AuthResponse
{
    public string AccessToken { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
    public DateTime ExpiresAtUtc { get; init; }
    public AuthUserDto User { get; init; } = new();
}

public sealed class AuthUserDto
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public IReadOnlyList<string> Roles { get; init; } = [];
}
