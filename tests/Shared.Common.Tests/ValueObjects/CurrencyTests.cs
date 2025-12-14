using Shared.Common.ValueObjects;

namespace Shared.Common.Tests.ValueObjects;

public sealed class CurrencyTests
{
    [Fact]
    public void Create_WithValidThreeLetterCode_ReturnsCurrency()
    {
        var currency = Currency.Create("USD");

        Assert.Equal("USD", currency.Code);
    }

    [Fact]
    public void Create_WithLowerCaseCode_ConvertsToUpperCase()
    {
        var currency = Currency.Create("usd");

        Assert.Equal("USD", currency.Code);
    }

    [Fact]
    public void Create_WithInvalidLength_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => Currency.Create("US"));
        Assert.Throws<ArgumentException>(() => Currency.Create("USDD"));
    }

    [Fact]
    public void Create_WithNullCode_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => Currency.Create(null!));
        Assert.Throws<ArgumentException>(() => Currency.Create(string.Empty));
        Assert.Throws<ArgumentException>(() => Currency.Create("   "));
    }

    [Fact]
    public void USD_ReturnsCorrectCurrency()
    {
        var currency = Currency.USD;

        Assert.Equal("USD", currency.Code);
    }

    [Fact]
    public void EUR_ReturnsCorrectCurrency()
    {
        var currency = Currency.EUR;

        Assert.Equal("EUR", currency.Code);
    }

    [Fact]
    public void TRY_ReturnsCorrectCurrency()
    {
        var currency = Currency.TRY;

        Assert.Equal("TRY", currency.Code);
    }

    [Fact]
    public void Equals_WithSameCode_ReturnsTrue()
    {
        var currency1 = Currency.Create("USD");
        var currency2 = Currency.Create("USD");

        Assert.True(currency1.Equals(currency2));
        Assert.True(currency1 == currency2);
        Assert.False(currency1 != currency2);
    }

    [Fact]
    public void Equals_WithDifferentCode_ReturnsFalse()
    {
        var currency1 = Currency.Create("USD");
        var currency2 = Currency.Create("EUR");

        Assert.False(currency1.Equals(currency2));
        Assert.False(currency1 == currency2);
        Assert.True(currency1 != currency2);
    }

    [Fact]
    public void Equals_WithCaseInsensitiveCode_ReturnsTrue()
    {
        var currency1 = Currency.Create("USD");
        var currency2 = Currency.Create("usd");

        Assert.True(currency1.Equals(currency2));
    }

    [Fact]
    public void GetHashCode_WithSameCode_ReturnsSameHash()
    {
        var currency1 = Currency.Create("USD");
        var currency2 = Currency.Create("USD");

        var hash1 = currency1.GetHashCode();
        var hash2 = currency2.GetHashCode();

        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void ToString_ReturnsCurrencyCode()
    {
        var currency = Currency.Create("USD");

        var result = currency.ToString();

        Assert.Equal("USD", result);
    }
}