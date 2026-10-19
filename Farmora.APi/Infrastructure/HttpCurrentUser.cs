using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Farmora.Application.Common;

namespace Farmora.APi.Infrastructure;

public sealed class HttpCurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    public Guid? UserId
    {
        get
        {
            var user = accessor.HttpContext?.User;
            if (user is null) return null;
            var raw = user.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? user.FindFirstValue(JwtRegisteredClaimNames.Sub)
                ?? user.FindFirstValue("sub");
            return Guid.TryParse(raw, out var id) ? id : null;
        }
    }

    public string? Email
    {
        get
        {
            var user = accessor.HttpContext?.User;
            if (user is null) return null;
            return user.FindFirstValue(ClaimTypes.Email)
                ?? user.FindFirstValue(JwtRegisteredClaimNames.Email)
                ?? user.FindFirstValue("email");
        }
    }

    public IReadOnlyList<string> Roles
    {
        get
        {
            var user = accessor.HttpContext?.User;
            if (user is null) return [];
            return user.FindAll(ClaimTypes.Role).Select(c => c.Value)
                .Concat(user.FindAll("role").Select(c => c.Value))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
    }

    public bool IsAuthenticated => accessor.HttpContext?.User.Identity?.IsAuthenticated == true;
}
