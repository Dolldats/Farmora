using Farmora.Domain.Enums;

namespace Farmora.Application.DTO.Account;

public sealed class MyProfileResponse
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string? Phone { get; init; }
    public UserStatus Status { get; init; }
    public bool IsEmailConfirmed { get; init; }
    public IReadOnlyList<string> Roles { get; init; } = [];
}
