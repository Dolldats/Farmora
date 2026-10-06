namespace Farmora.Application.DTO.User;

public sealed class UpdateUserRequest
{
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string Password { get; set; } = string.Empty;
}
