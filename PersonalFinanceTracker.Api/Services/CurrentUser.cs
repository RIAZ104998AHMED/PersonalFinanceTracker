using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace PersonalFinanceTracker.Api.Services;

public sealed class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(
        IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId
    {
        get
        {
            var user =
                _httpContextAccessor
                    .HttpContext?
                    .User;

            if (user is null)
            {
                return null;
            }

            var value =
                user.FindFirst(
                    JwtRegisteredClaimNames.Sub)?.Value
                ?? user.FindFirst(
                    ClaimTypes.NameIdentifier)?.Value;

            return Guid.TryParse(
                value,
                out var userId)
                ? userId
                : null;
        }
    }
}