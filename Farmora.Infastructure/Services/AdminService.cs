using Farmora.Application.Common;
using Farmora.Application.DTO.Admin;
using Farmora.Application.Interfaces.Services;
using Farmora.Domain.Entities;
using Farmora.Domain.Enums;
using Farmora.Infastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Farmora.Infastructure.Services;

public sealed class AdminService(FarmoraDbContext db) : IAdminService
{
    public async Task<PagedResult<UserAdminDto>> GetUsersAsync(
        PaginationParams pagination, string? search, CancellationToken cancellationToken = default)
    {
        var query = db.Users.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLowerInvariant();
            query = query.Where(u => u.Email.Contains(s) || (u.Phone != null && u.Phone.Contains(s)));
        }

        var total = await query.CountAsync(cancellationToken);
        var users = await query.OrderBy(u => u.CreatedAtUtc)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync(cancellationToken);

        var ids = users.Select(u => u.Id).ToList();
        var rolesByUser = await db.Set<UserRole>().AsNoTracking()
            .Where(ur => ids.Contains(ur.UserId))
            .Join(db.Set<Role>().AsNoTracking(), ur => ur.RoleId, r => r.Id,
                (ur, r) => new { ur.UserId, r.Name })
            .GroupBy(x => x.UserId)
            .ToDictionaryAsync(g => g.Key, g => (IReadOnlyList<string>)g.Select(x => x.Name).ToList(), cancellationToken);

        var items = users.Select(u => new UserAdminDto
        {
            Id = u.Id,
            Email = u.Email,
            Phone = u.Phone,
            Status = u.Status,
            IsEmailConfirmed = u.IsEmailConfirmed,
            Roles = rolesByUser.TryGetValue(u.Id, out var roles) ? roles : [],
            CreatedAtUtc = u.CreatedAtUtc,
        }).ToList();

        return PagedResult<UserAdminDto>.Create(items, total, pagination.Page, pagination.PageSize);
    }

    public async Task<UserAdminDto?> SetRoleAsync(Guid userId, SetRoleRequest request, CancellationToken cancellationToken = default)
    {
        var user = await db.Users.SingleOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user is null)
            return null;
        if (string.IsNullOrWhiteSpace(request.Role))
            throw new ArgumentException("Role is required.");
        var roleName = request.Role.Trim();

        var role = await db.Set<Role>().SingleOrDefaultAsync(r => r.Name == roleName, cancellationToken);
        if (role is null)
        {
            role = new Role(roleName);
            db.Set<Role>().Add(role);
            await db.SaveChangesAsync(cancellationToken);
        }

        var existing = await db.Set<UserRole>().Where(ur => ur.UserId == userId).ToListAsync(cancellationToken);
        db.Set<UserRole>().RemoveRange(existing);
        db.Set<UserRole>().Add(new UserRole(userId, role.Id));

        db.Set<AuditLog>().Add(new AuditLog(null, "SetRole", "User", userId, $"Role set to {roleName}"));
        await db.SaveChangesAsync(cancellationToken);
        return await MapAsync(user, cancellationToken);
    }

    public async Task<UserAdminDto?> SetUserStatusAsync(Guid userId, SetUserStatusRequest request, CancellationToken cancellationToken = default)
    {
        var user = await db.Users.SingleOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user is null)
            return null;

        switch (request.Status)
        {
            case UserStatus.Active:
                user.Activate();
                break;
            case UserStatus.Disabled:
                user.Disable();
                break;
            default:
                db.Entry(user).Property(u => u.Status).CurrentValue = request.Status;
                user.MarkUpdated();
                break;
        }
        user.BumpSecurityStamp();

        if (request.Status == UserStatus.Disabled)
        {
            var activeTokens = await db.Set<RefreshToken>()
                .Where(t => t.UserId == userId && t.RevokedAtUtc == null)
                .ToListAsync(cancellationToken);
            foreach (var token in activeTokens)
                token.Revoke();
        }

        db.Set<AuditLog>().Add(new AuditLog(null, "SetUserStatus", "User", userId, $"Status set to {request.Status}"));
        await db.SaveChangesAsync(cancellationToken);
        return await MapAsync(user, cancellationToken);
    }

    public async Task<PagedResult<AuditLogDto>> GetAuditLogsAsync(
        PaginationParams pagination, CancellationToken cancellationToken = default)
    {
        var query = db.Set<AuditLog>().AsNoTracking();
        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(a => a.CreatedAtUtc)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(a => new AuditLogDto
            {
                Id = a.Id,
                ActorId = a.ActorId,
                Action = a.Action,
                Resource = a.Resource,
                ResourceId = a.ResourceId,
                Detail = a.Detail,
                CreatedAtUtc = a.CreatedAtUtc,
            })
            .ToListAsync(cancellationToken);
        return PagedResult<AuditLogDto>.Create(items, total, pagination.Page, pagination.PageSize);
    }

    private async Task<UserAdminDto> MapAsync(User user, CancellationToken ct)
    {
        var roles = await db.Set<UserRole>().AsNoTracking()
            .Where(ur => ur.UserId == user.Id)
            .Join(db.Set<Role>().AsNoTracking(), ur => ur.RoleId, r => r.Id, (_, r) => r.Name)
            .ToListAsync(ct);
        return new UserAdminDto
        {
            Id = user.Id,
            Email = user.Email,
            Phone = user.Phone,
            Status = user.Status,
            IsEmailConfirmed = user.IsEmailConfirmed,
            Roles = roles,
            CreatedAtUtc = user.CreatedAtUtc,
        };
    }
}
