using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Shared.Common.Identity;

namespace Shared.Infrastructure.Authentication;

public sealed class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? Name
    {
        get
        {
            var httpContext = _httpContextAccessor.HttpContext;
            return httpContext?.User.Identity?.Name;
        }
    }

    public string GetUserId()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            return string.Empty;
        }

        var userIdClaim = httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                          ?? httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        return userIdClaim ?? string.Empty;
    }

    public string? GetUserEmail()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            return null;
        }

        return httpContext.User.FindFirst(JwtRegisteredClaimNames.Email)?.Value
               ?? httpContext.User.FindFirst(ClaimTypes.Email)?.Value;
    }

    public bool IsAuthenticated()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            return false;
        }

        return httpContext.User.Identity?.IsAuthenticated ?? false;
    }

    public IEnumerable<Claim>? GetUserClaims()
    {
        var httpContext = _httpContextAccessor.HttpContext;

        return httpContext?.User.Claims;
    }
}