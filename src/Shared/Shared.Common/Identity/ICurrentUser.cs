using System.Security.Claims;

namespace Shared.Common.Identity;

public interface ICurrentUser
{
    string? Name { get; }

    string GetUserId();

    string? GetUserEmail();

    bool IsAuthenticated();

    IEnumerable<Claim>? GetUserClaims();
}