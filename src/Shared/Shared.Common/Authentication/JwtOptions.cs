namespace Shared.Common.Authentication;

public sealed class JwtOptions
{
    public string SecretKey { get; init; } = null!;
    public string Issuer { get; init; } = null!;
    public string Audience { get; init; } = null!;
    public int ExpirationMinutes { get; init; } = 60;
}