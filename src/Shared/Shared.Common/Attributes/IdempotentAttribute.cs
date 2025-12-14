namespace Shared.Common.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class IdempotentAttribute : Attribute
{
    public int ExpirationHours { get; set; } = 24;

    public bool Required { get; set; } = true;

    public string HeaderName { get; set; } = "X-Idempotency-Key";
}