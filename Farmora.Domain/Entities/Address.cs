using Farmora.Domain.Common;

namespace Farmora.Domain.Entities;

public sealed class Address : AuditableEntity
{
    private Address() { }
    public Address(Guid customerId, string recipient, string line1, string city, string state, string country)
    {
        CustomerId = customerId;
        RecipientName = recipient.Trim();
        Line1 = line1.Trim();
        City = city.Trim();
        State = state.Trim();
        Country = country.Trim();
    }

    public Guid CustomerId { get; private set; }
    public string RecipientName { get; private set; } = null!;
    public string Line1 { get; private set; } = null!;
    public string? Line2 { get; private set; }
    public string City { get; private set; } = null!;
    public string State { get; private set; } = null!;
    public string Country { get; private set; } = "Nigeria";
    public string? PostalCode { get; private set; }
    public string? Phone { get; private set; }
    public bool IsDefault { get; private set; }
    public Customer Customer { get; private set; } = null!;

    public void Update(string recipient, string line1, string? line2, string city, string state, string? postal, string? phone)
    {
        RecipientName = recipient.Trim();
        Line1 = line1.Trim();
        Line2 = line2?.Trim();
        City = city.Trim();
        State = state.Trim();
        PostalCode = postal?.Trim();
        Phone = phone?.Trim();
        MarkUpdated();
    }

    public void SetDefault(bool isDefault) { IsDefault = isDefault; MarkUpdated(); }

    public string Snapshot() => $"{RecipientName}, {Line1}{(Line2 is null ? "" : ", " + Line2)}, {City}, {State}, {Country}";
}
