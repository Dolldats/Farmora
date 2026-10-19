using Farmora.Application.DTO.Account;
using Farmora.Application.Interfaces.Services;
using Farmora.Domain.Entities;
using Farmora.Infastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Farmora.Infastructure.Services;

public sealed class AccountService(FarmoraDbContext db) : IAccountService
{
    public async Task<MyProfileResponse?> GetMeAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await db.Users.AsNoTracking().SingleOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user is null)
            return null;
        var roles = await GetRolesAsync(userId, cancellationToken);
        return new MyProfileResponse
        {
            Id = user.Id,
            Email = user.Email,
            Phone = user.Phone,
            Status = user.Status,
            IsEmailConfirmed = user.IsEmailConfirmed,
            Roles = roles,
        };
    }

    public async Task<MyProfileResponse?> UpdateMeAsync(Guid userId, UpdateMyProfileDto dto, CancellationToken cancellationToken = default)
    {
        var user = await db.Users.SingleOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user is null)
            return null;

        if (!string.IsNullOrWhiteSpace(dto.Email))
        {
            var email = dto.Email.Trim().ToLowerInvariant();
            if (!email.Equals(user.Email, StringComparison.OrdinalIgnoreCase))
            {
                if (await db.Users.AnyAsync(u => u.Email == email && u.Id != userId, cancellationToken))
                    throw new InvalidOperationException("A user with this email already exists.");
                db.Entry(user).Property(u => u.Email).CurrentValue = email;
                user.BumpSecurityStamp();
            }
        }

        if (dto.Phone is not null)
            user.UpdatePhone(dto.Phone.Trim());

        if (!string.IsNullOrWhiteSpace(dto.DisplayName))
        {
            var customer = await db.Set<Customer>().SingleOrDefaultAsync(c => c.UserId == userId, cancellationToken);
            customer?.UpdateProfile(dto.DisplayName.Trim(), customer.PreferencesJson);
        }

        await db.SaveChangesAsync(cancellationToken);
        return await GetMeAsync(userId, cancellationToken);
    }

    public async Task<CustomerResponse?> GetMyCustomerAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var customer = await db.Set<Customer>().AsNoTracking()
            .SingleOrDefaultAsync(c => c.UserId == userId, cancellationToken);
        return customer is null ? null : Map(customer);
    }

    public async Task<CustomerResponse> UpsertMyCustomerAsync(Guid userId, UpdateCustomerDto dto, CancellationToken cancellationToken = default)
    {
        var user = await db.Users.SingleOrDefaultAsync(u => u.Id == userId, cancellationToken)
            ?? throw new KeyNotFoundException("User not found.");
        _ = user;

        var customer = await db.Set<Customer>().SingleOrDefaultAsync(c => c.UserId == userId, cancellationToken);
        if (customer is null)
        {
            customer = new Customer(userId);
            db.Set<Customer>().Add(customer);
        }
        customer.UpdateProfile(dto.DisplayName, dto.PreferencesJson);
        await db.SaveChangesAsync(cancellationToken);
        return Map(customer);
    }

    public async Task<FarmerResponse?> GetMyFarmerAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var farmer = await db.Set<Farmer>().AsNoTracking()
            .SingleOrDefaultAsync(f => f.UserId == userId, cancellationToken);
        return farmer is null ? null : Map(farmer);
    }

    public async Task<FarmerResponse> UpsertMyFarmerAsync(Guid userId, UpdateFarmerDto dto, CancellationToken cancellationToken = default)
    {
        var user = await db.Users.SingleOrDefaultAsync(u => u.Id == userId, cancellationToken)
            ?? throw new KeyNotFoundException("User not found.");
        _ = user;
        if (string.IsNullOrWhiteSpace(dto.FarmName))
            throw new ArgumentException("Farm name is required.");

        var farmer = await db.Set<Farmer>().SingleOrDefaultAsync(f => f.UserId == userId, cancellationToken);
        if (farmer is null)
        {
            farmer = new Farmer(userId, dto.FarmName.Trim());
            db.Set<Farmer>().Add(farmer);
            await db.SaveChangesAsync(cancellationToken);
        }

        var entry = db.Entry(farmer);
        entry.Property(f => f.FarmName).CurrentValue = dto.FarmName.Trim();
        entry.Property(f => f.Description).CurrentValue = dto.Description?.Trim();
        entry.Property(f => f.Address).CurrentValue = dto.Address?.Trim();
        entry.Property(f => f.Latitude).CurrentValue = dto.Latitude;
        entry.Property(f => f.Longitude).CurrentValue = dto.Longitude;
        farmer.MarkUpdated();
        await db.SaveChangesAsync(cancellationToken);
        return Map(farmer);
    }

    public async Task<bool> DeleteMyCustomerAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var customer = await db.Set<Customer>().SingleOrDefaultAsync(c => c.UserId == userId, cancellationToken);
        if (customer is null || customer.IsDeleted)
            return false;
        customer.Deactivate();
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<IReadOnlyList<string>> GetRolesAsync(Guid userId, CancellationToken ct) =>
        await db.Set<UserRole>().AsNoTracking()
            .Where(ur => ur.UserId == userId)
            .Join(db.Set<Role>().AsNoTracking(), ur => ur.RoleId, r => r.Id, (_, r) => r.Name)
            .ToListAsync(ct);

    private static CustomerResponse Map(Customer c) => new()
    {
        UserId = c.UserId,
        DisplayName = c.DisplayName,
        PreferencesJson = c.PreferencesJson,
        IsDeleted = c.IsDeleted,
        DeletedAtUtc = c.DeletedAtUtc,
    };

    private static FarmerResponse Map(Farmer f) => new()
    {
        UserId = f.UserId,
        FarmName = f.FarmName,
        Description = f.Description,
        Address = f.Address,
        Latitude = f.Latitude,
        Longitude = f.Longitude,
        VerificationStatus = f.VerificationStatus,
        OperationalStatus = f.OperationalStatus,
    };
}
