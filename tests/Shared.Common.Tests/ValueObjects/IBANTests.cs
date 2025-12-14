using Shared.Common.ValueObjects;

namespace Shared.Common.Tests.ValueObjects;

public sealed class IBANTests
{
    [Theory]
    [InlineData("TR330006100519786457841326")]
    [InlineData("GB82WEST12345698765432")]
    public void Create_WithValidIBAN_ReturnsIBAN(string ibanValue)
    {
        var iban = IBAN.Create(ibanValue);

        Assert.NotNull(iban);
        Assert.NotEmpty(iban.Value);
    }

    [Fact]
    public void Create_WithSpaces_RemovesSpaces()
    {
        var ibanWithSpaces = "TR33 0006 1005 1978 6457 8413 26";

        var iban = IBAN.Create(ibanWithSpaces);

        Assert.DoesNotContain(" ", iban.Value);
    }

    [Fact]
    public void Create_WithHyphens_RemovesHyphens()
    {
        var ibanWithHyphens = "TR33-0006-1005-1978-6457-8413-26";

        var iban = IBAN.Create(ibanWithHyphens);

        Assert.DoesNotContain("-", iban.Value);
    }

    [Fact]
    public void Create_WithLowerCase_ConvertsToUpperCase()
    {
        var lowerCaseIban = "tr330006100519786457841326";

        var iban = IBAN.Create(lowerCaseIban);

        Assert.Equal("TR330006100519786457841326", iban.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithNullOrEmpty_ThrowsArgumentException(string? invalidIban)
    {
        Assert.Throws<ArgumentException>(() => IBAN.Create(invalidIban!));
    }

    [Theory]
    [InlineData("1234567890")]
    [InlineData("TR123")]
    [InlineData("INVALID")]
    public void Create_WithInvalidFormat_ThrowsArgumentException(string invalidIban)
    {
        Assert.Throws<ArgumentException>(() => IBAN.Create(invalidIban));
    }

    [Fact]
    public void Create_WithInvalidChecksum_ThrowsArgumentException()
    {
        var invalidChecksumIban = "TR990006100519786457841326";

        Assert.Throws<ArgumentException>(() => IBAN.Create(invalidChecksumIban));
    }

    [Fact]
    public void Equals_WithSameValue_ReturnsTrue()
    {
        var iban1 = IBAN.Create("TR330006100519786457841326");
        var iban2 = IBAN.Create("TR330006100519786457841326");

        Assert.True(iban1.Equals(iban2));
    }

    [Fact]
    public void Equals_WithDifferentValue_ReturnsFalse()
    {
        var iban1 = IBAN.Create("TR330006100519786457841326");
        var iban2 = IBAN.Create("GB82WEST12345698765432");

        Assert.False(iban1.Equals(iban2));
    }

    [Fact]
    public void ToFormattedString_ReturnsFormattedIBAN()
    {
        var iban = IBAN.Create("TR330006100519786457841326");

        var formatted = iban.ToFormattedString();

        Assert.Contains(" ", formatted);
        Assert.Equal("TR33 0006 1005 1978 6457 8413 26", formatted);
    }
}