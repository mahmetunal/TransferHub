namespace Shared.Common.ValueObjects;

public sealed class Currency : IEquatable<Currency>
{
    public string Code { get; }

    private Currency()
    {
        Code = "USD";
    }

    private Currency(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Currency code cannot be null or empty", nameof(code));

        if (code.Length != 3)
            throw new ArgumentException("Currency code must be exactly 3 characters (ISO 4217)", nameof(code));

        Code = code.ToUpperInvariant();
    }

    public static Currency Create(string code)
    {
        return new Currency(code);
    }

    public static Currency USD => new("USD");
    public static Currency EUR => new("EUR");
    public static Currency GBP => new("GBP");
    public static Currency TRY => new("TRY");

    public override bool Equals(object? obj)
    {
        return Equals(obj as Currency);
    }

    public bool Equals(Currency? other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        return Code == other.Code;
    }

    public override int GetHashCode()
    {
        return Code.GetHashCode(StringComparison.OrdinalIgnoreCase);
    }

    public static bool operator ==(Currency? left, Currency? right)
    {
        if (left is null && right is null)
            return true;

        if (left is null || right is null)
            return false;

        return left.Equals(right);
    }

    public static bool operator !=(Currency? left, Currency? right)
    {
        return !(left == right);
    }

    public override string ToString()
    {
        return Code;
    }
}