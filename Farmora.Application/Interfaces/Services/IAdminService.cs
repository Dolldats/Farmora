using Farmora.Application.Common;
using Farmora.Application.DTO.Admin;

namespace Farmora.Application.Interfaces.Services;

public interface IAdminService
{
    Task<PagedResult<UserAdminDto>> GetUsersAsync(PaginationParams pagination, string? search, CancellationToken cancellationToken = default);
    Task<UserAdminDto?> SetRoleAsync(Guid userId, SetRoleRequest request, CancellationToken cancellationToken = default);
    Task<UserAdminDto?> SetUserStatusAsync(Guid userId, SetUserStatusRequest request, CancellationToken cancellationToken = default);
    Task<PagedResult<AuditLogDto>> GetAuditLogsAsync(PaginationParams pagination, CancellationToken cancellationToken = default);
}
