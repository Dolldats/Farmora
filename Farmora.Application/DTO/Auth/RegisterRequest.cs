namespace Farmora.Application.DTO.Auth;

public sealed class RegisterRequest
{
    public string Email { get; init; } = string.Empty;
    public string? Phone { get; init; }
    public string Password { get; init; } = string.Empty;

    /// <summary>Customer or Farmer. Defaults to Customer.</summary>
    public string Role { get; init; } = "Customer";
}
