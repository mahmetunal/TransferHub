namespace Shared.Common.Authentication.Models;

public sealed class LoginRequest
{
    public string Username { get; init; } = null!;
    public string Password { get; init; } = null!;
}