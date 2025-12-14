namespace Shared.Common.ValueObjects;

public sealed class Money : IEquatable<Money>
{
    public decimal Amount { get; }
    public Currency Currency { get; }

    private Money()
    {
        Amount = 0;
        Currency = Currency.TRY;
    }

    private Money(decimal amount, Currency currency)
    {
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative", nameof(amount));

        Amount = amount;
        Currency = currency ?? throw new ArgumentNullException(nameof(currency));
    }

    public static Money Create(decimal amount, Currency currency)
    {
        return new Money(amount, currency);
    }

    public static Money Zero(Currency currency)
    {
        return new Money(0, currency);
    }

    public Money Add(Money other)
    {
        ArgumentNullException.ThrowIfNull(other);

        return Currency != other.Currency ? throw new InvalidOperationException($"Cannot add money with different currencies. Current: {Currency.Code}, Other: {other.Currency.Code}")
            : new Money(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        ArgumentNullException.ThrowIfNull(other);

        if (Currency != other.Currency)
            throw new InvalidOperationException($"Cannot subtract money with different currencies. Current: {Currency.Code}, Other: {other.Currency.Code}");

        var result = Amount - other.Amount;

        return result < 0 ? throw new InvalidOperationException("Result cannot be negative") : new Money(result, Currency);
    }

    public bool IsGreaterThan(Money other)
    {
        ArgumentNullException.ThrowIfNull(other);

        if (Currency != other.Currency)
            throw new InvalidOperationException($"Cannot compare money with different currencies. Current: {Currency.Code}, Other: {other.Currency.Code}");

        return Amount > other.Amount;
    }

    public bool IsLessThan(Money other)
    {
        ArgumentNullException.ThrowIfNull(other);

        if (Currency != other.Currency)
            throw new InvalidOperationException($"Cannot compare money with different currencies. Current: {Currency.Code}, Other: {other.Currency.Code}");

        return Amount < other.Amount;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as Money);
    }

    public bool Equals(Money? other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        return Amount == other.Amount && Currency == other.Currency;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Amount, Currency);
    }

    public static bool operator ==(Money? left, Money? right)
    {
        if (left is null && right is null)
            return true;

        if (left is null || right is null)
            return false;

        return left.Equals(right);
    }

    public static bool operator !=(Money? left, Money? right)
    {
        return !(left == right);
    }

    public override string ToString()
    {
        return $"{Amount:F2} {Currency.Code}";
    }
}