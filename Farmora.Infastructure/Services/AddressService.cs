using Farmora.Application.DTO.Address;
using Farmora.Application.Interfaces.Repositories;
using Farmora.Application.Interfaces.Services;
using Farmora.Domain.Entities;
using Farmora.Infastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Farmora.Infastructure.Services;

public sealed class AddressService(IAddressRepository addresses, FarmoraDbContext db) : IAddressService
{
    public async Task<IReadOnlyList<AddressResponse>> ListAsync(Guid customerId, CancellationToken cancellationToken)
    {
        var customer = await RequireCustomerAsync(customerId, cancellationToken);
        var list = await addresses.ListByCustomerAsync(customer.Id, cancellationToken);
        return list.Select(Map).ToList();
    }

    public async Task<AddressResponse?> GetAsync(Guid customerId, Guid addressId, CancellationToken cancellationToken)
    {
        var customer = await RequireCustomerAsync(customerId, cancellationToken);
        var address = await addresses.GetByIdAsync(addressId, cancellationToken);
        if (address is null || address.CustomerId != customer.Id) return null;
        return Map(address);
    }

    public async Task<AddressResponse> CreateAsync(Guid customerId, CreateAddressDto dto, CancellationToken cancellationToken)
    {
        var customer = await RequireCustomerAsync(customerId, cancellationToken);
        var address = new Address(customer.Id, dto.RecipientName, dto.Line1, dto.City, dto.State, dto.Country);
        address.Update(dto.RecipientName, dto.Line1, dto.Line2, dto.City, dto.State, dto.PostalCode, dto.Phone);

        if (dto.IsDefault)
            await UnsetDefaultsAsync(customer.Id, cancellationToken);

        await addresses.AddAsync(address, cancellationToken);
        await addresses.SaveChangesAsync(cancellationToken);

        if (dto.IsDefault)
        {
            address.SetDefault(true);
            await addresses.SaveChangesAsync(cancellationToken);
        }

        return Map(address);
    }

    public async Task<AddressResponse?> UpdateAsync(Guid customerId, Guid addressId, UpdateAddressDto dto, CancellationToken cancellationToken)
    {
        var customer = await RequireCustomerAsync(customerId, cancellationToken);
        var address = await addresses.GetByIdAsync(addressId, cancellationToken);
        if (address is null || address.CustomerId != customer.Id) return null;
        address.Update(dto.RecipientName, dto.Line1, dto.Line2, dto.City, dto.State, dto.PostalCode, dto.Phone);
        await addresses.SaveChangesAsync(cancellationToken);
        return Map(address);
    }

    public async Task<bool> DeleteAsync(Guid customerId, Guid addressId, CancellationToken cancellationToken)
    {
        var customer = await RequireCustomerAsync(customerId, cancellationToken);
        var address = await addresses.GetByIdAsync(addressId, cancellationToken);
        if (address is null || address.CustomerId != customer.Id) return false;
        await addresses.RemoveAsync(address, cancellationToken);
        await addresses.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> SetDefaultAsync(Guid customerId, Guid addressId, CancellationToken cancellationToken)
    {
        var customer = await RequireCustomerAsync(customerId, cancellationToken);
        var address = await addresses.GetByIdAsync(addressId, cancellationToken);
        if (address is null || address.CustomerId != customer.Id) return false;
        await UnsetDefaultsAsync(customer.Id, cancellationToken);
        address.SetDefault(true);
        await addresses.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task UnsetDefaultsAsync(Guid customerId, CancellationToken ct)
    {
        var defaults = await db.Set<Address>()
            .Where(a => a.CustomerId == customerId && a.IsDefault)
            .ToListAsync(ct);
        foreach (var d in defaults)
            d.SetDefault(false);
    }

    private async Task<Customer> RequireCustomerAsync(Guid idOrUserId, CancellationToken ct) =>
        await db.Set<Customer>().FirstOrDefaultAsync(c => c.Id == idOrUserId, ct)
        ?? await db.Set<Customer>().FirstOrDefaultAsync(c => c.UserId == idOrUserId, ct)
        ?? throw new InvalidOperationException("Customer profile required.");

    private static AddressResponse Map(Address a) => new()
    {
        Id = a.Id,
        CustomerId = a.CustomerId,
        RecipientName = a.RecipientName,
        Line1 = a.Line1,
        Line2 = a.Line2,
        City = a.City,
        State = a.State,
        Country = a.Country,
        PostalCode = a.PostalCode,
        Phone = a.Phone,
        IsDefault = a.IsDefault,
    };
}
