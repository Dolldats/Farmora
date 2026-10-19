using Farmora.Domain.Entities;
using Farmora.Infastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Farmora.Infastructure.Identity;

/// <summary>
/// Ensures the configured super-admin exists at startup. Skipped when
/// AdminSeed:Email/Password are not configured. Public registration
/// intentionally cannot create Admin users (see AuthService).
/// </summary>
public static class AdminSeeder
{
    public static async Task SeedAsync(IServiceProvider services, CancellationToken ct = default)
    {
        using var scope = services.CreateScope();
        var provider = scope.ServiceProvider;
        var options = provider.GetRequiredService<IOptions<AdminSeedOptions>>().Value;

        var email = options.Email?.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(options.Password))
            return;

        var db = provider.GetRequiredService<FarmoraDbContext>();
        var hasher = provider.GetRequiredService<IPasswordHasher<User>>();

        var user = await db.Users.SingleOrDefaultAsync(u => u.Email == email, ct);
        if (user is null)
        {
            user = new User(email, null, string.Empty);
            user.UpdateProfile(email, null, hasher.HashPassword(user, options.Password));
            user.Activate();
            user.ConfirmEmail();
            db.Users.Add(user);
            await db.SaveChangesAsync(ct);
        }

        var role = await db.Set<Role>().SingleOrDefaultAsync(r => r.Name == "Admin", ct);
        if (role is null)
        {
            role = new Role("Admin");
            db.Set<Role>().Add(role);
            await db.SaveChangesAsync(ct);
        }

        var linked = await db.Set<UserRole>()
            .AnyAsync(ur => ur.UserId == user.Id && ur.RoleId == role.Id, ct);
        if (!linked)
        {
            db.Set<UserRole>().Add(new UserRole(user.Id, role.Id));
            await db.SaveChangesAsync(ct);
        }
    }
}
