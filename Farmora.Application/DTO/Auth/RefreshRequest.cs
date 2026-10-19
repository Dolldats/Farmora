namespace Farmora.Application.DTO.Auth;

public sealed class RefreshRequest
{
    public string RefreshToken { get; init; } = string.Empty;
}
