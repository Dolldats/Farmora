namespace Farmora.Application.DTO.Account;

public sealed class UpdateMyProfileDto
{
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? DisplayName { get; init; }
}
