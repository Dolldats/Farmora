using Farmora.Domain.Enums;

namespace Farmora.Application.DTO.Admin;

public sealed class SetUserStatusRequest
{
    public UserStatus Status { get; init; }
}
