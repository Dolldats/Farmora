namespace Farmora.Application.DTO.Address;

public sealed class AddressResponse
{
    public Guid Id { get; init; }
    public Guid CustomerId { get; init; }
    public string RecipientName { get; init; } = string.Empty;
    public string Line1 { get; init; } = string.Empty;
    public string? Line2 { get; init; }
    public string City { get; init; } = string.Empty;
    public string State { get; init; } = string.Empty;
    public string Country { get; init; } = string.Empty;
    public string? PostalCode { get; init; }
    public string? Phone { get; init; }
    public bool IsDefault { get; init; }
}
