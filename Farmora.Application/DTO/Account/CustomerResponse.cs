namespace Farmora.Application.DTO.Account;

public sealed class CustomerResponse
{
    public Guid UserId { get; init; }
    public string? DisplayName { get; init; }
    public string? PreferencesJson { get; init; }
    public bool IsDeleted { get; init; }
    public DateTime? DeletedAtUtc { get; init; }
}
