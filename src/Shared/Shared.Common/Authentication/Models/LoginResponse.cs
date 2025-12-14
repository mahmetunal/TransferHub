namespace Shared.Common.Authentication.Models;

public sealed class LoginResponse
{
    public string Token { get; init; } = null!;
    public string UserId { get; init; } = null!;
    public string Email { get; init; } = null!;
    public int ExpiresIn { get; init; }
    public string TokenType { get; init; } = null!;
}