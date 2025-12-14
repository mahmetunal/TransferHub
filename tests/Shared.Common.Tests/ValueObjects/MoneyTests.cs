using Shared.Common.ValueObjects;

namespace Shared.Common.Tests.ValueObjects;

public sealed class MoneyTests
{
    [Fact]
    public void Create_WithValidAmountAndCurrency_ReturnsMoney()
    {
        var amount = 100.50m;
        var currency = Currency.Create("USD");

        var money = Money.Create(amount, currency);

        Assert.Equal(amount, money.Amount);
        Assert.Equal(currency, money.Currency);
    }

    [Fact]
    public void Create_WithNegativeAmount_ThrowsArgumentException()
    {
        var amount = -10m;
        var currency = Currency.Create("USD");

        Assert.Throws<ArgumentException>(() => Money.Create(amount, currency));
    }

    [Fact]
    public void Create_WithNullCurrency_ThrowsArgumentNullException()
    {
        var amount = 100m;

        Assert.Throws<ArgumentNullException>(() => Money.Create(amount, null!));
    }

    [Fact]
    public void Zero_WithValidCurrency_ReturnsZeroAmount()
    {
        var currency = Currency.Create("EUR");

        var money = Money.Zero(currency);

        Assert.Equal(0m, money.Amount);
        Assert.Equal(currency, money.Currency);
    }

    [Fact]
    public void Add_WithSameCurrency_ReturnsSum()
    {
        var money1 = Money.Create(100m, Currency.Create("USD"));
        var money2 = Money.Create(50m, Currency.Create("USD"));

        var result = money1.Add(money2);

        Assert.Equal(150m, result.Amount);
        Assert.Equal(Currency.Create("USD"), result.Currency);
    }

    [Fact]
    public void Add_WithDifferentCurrencies_ThrowsInvalidOperationException()
    {
        var money1 = Money.Create(100m, Currency.Create("USD"));
        var money2 = Money.Create(50m, Currency.Create("EUR"));

        Assert.Throws<InvalidOperationException>(() => money1.Add(money2));
    }

    [Fact]
    public void Add_WithNullMoney_ThrowsArgumentNullException()
    {
        var money = Money.Create(100m, Currency.Create("USD"));

        Assert.Throws<ArgumentNullException>(() => money.Add(null!));
    }

    [Fact]
    public void Subtract_WithSameCurrency_ReturnsDifference()
    {
        var money1 = Money.Create(100m, Currency.Create("USD"));
        var money2 = Money.Create(30m, Currency.Create("USD"));

        var result = money1.Subtract(money2);

        Assert.Equal(70m, result.Amount);
        Assert.Equal(Currency.Create("USD"), result.Currency);
    }

    [Fact]
    public void Subtract_WithDifferentCurrencies_ThrowsInvalidOperationException()
    {
        var money1 = Money.Create(100m, Currency.Create("USD"));
        var money2 = Money.Create(30m, Currency.Create("EUR"));

        Assert.Throws<InvalidOperationException>(() => money1.Subtract(money2));
    }

    [Fact]
    public void Subtract_WithResultNegative_ThrowsInvalidOperationException()
    {
        var money1 = Money.Create(50m, Currency.Create("USD"));
        var money2 = Money.Create(100m, Currency.Create("USD"));

        Assert.Throws<InvalidOperationException>(() => money1.Subtract(money2));
    }

    [Fact]
    public void IsGreaterThan_WithGreaterAmount_ReturnsTrue()
    {
        var money1 = Money.Create(100m, Currency.Create("USD"));
        var money2 = Money.Create(50m, Currency.Create("USD"));

        var result = money1.IsGreaterThan(money2);

        Assert.True(result);
    }

    [Fact]
    public void IsGreaterThan_WithLesserAmount_ReturnsFalse()
    {
        var money1 = Money.Create(50m, Currency.Create("USD"));
        var money2 = Money.Create(100m, Currency.Create("USD"));

        var result = money1.IsGreaterThan(money2);

        Assert.False(result);
    }

    [Fact]
    public void Equals_WithSameAmountAndCurrency_ReturnsTrue()
    {
        var money1 = Money.Create(100m, Currency.Create("USD"));
        var money2 = Money.Create(100m, Currency.Create("USD"));

        var result = money1.Equals(money2);

        Assert.True(result);
    }

    [Fact]
    public void Equals_WithDifferentAmount_ReturnsFalse()
    {
        var money1 = Money.Create(100m, Currency.Create("USD"));
        var money2 = Money.Create(200m, Currency.Create("USD"));

        var result = money1.Equals(money2);

        Assert.False(result);
    }
}