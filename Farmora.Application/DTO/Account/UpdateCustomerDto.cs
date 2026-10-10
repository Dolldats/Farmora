namespace Farmora.Application.DTO.Account;

public sealed class UpdateCustomerDto
{
    public string? DisplayName { get; init; }
    public string? PreferencesJson { get; init; }
}
