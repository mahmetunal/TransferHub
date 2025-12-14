using System.Text.RegularExpressions;

namespace Shared.Common.ValueObjects;

public sealed class IBAN : IEquatable<IBAN>
{
    private static readonly Regex IbanPattern = new(@"^[A-Z]{2}\d{2}[A-Z0-9]{4,30}$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string Value { get; }

    private IBAN(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("IBAN cannot be null or empty", nameof(value));

        var normalized = value.Replace(" ", "").Replace("-", "").ToUpperInvariant();

        if (!IbanPattern.IsMatch(normalized))
            throw new ArgumentException($"Invalid IBAN format: {value}", nameof(value));

        if (!IsValidChecksum(normalized))
            throw new ArgumentException($"Invalid IBAN checksum: {value}", nameof(value));

        Value = normalized;
    }

    public static IBAN Create(string value)
    {
        return new IBAN(value);
    }

    public static IBAN Generate(string countryCode = "TR")
    {
        if (string.IsNullOrWhiteSpace(countryCode) || countryCode.Length != 2)
            throw new ArgumentException("Country code must be exactly 2 letters", nameof(countryCode));

        countryCode = countryCode.ToUpperInvariant();

        var accountNumber = countryCode switch
        {
            "TR" => GenerateRandomDigits(5) + "0" + GenerateRandomDigits(16),
            "DE" => GenerateRandomDigits(18),
            "GB" => GenerateRandomAlphanumeric(4) + GenerateRandomDigits(14),
            _ => GenerateRandomDigits(20)
        };

        var ibanWithoutChecksum = countryCode + "00" + accountNumber;

        var checkDigits = CalculateCheckDigits(ibanWithoutChecksum);

        var finalIban = countryCode + checkDigits + accountNumber;

        return new IBAN(finalIban);
    }

    private static string GenerateRandomDigits(int length)
    {
        var random = new Random(Guid.NewGuid().GetHashCode());
        var digits = new char[length];
        for (int i = 0; i < length; i++)
        {
            digits[i] = (char) ('0' + random.Next(0, 10));
        }

        return new string(digits);
    }

    private static string GenerateRandomAlphanumeric(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random(Guid.NewGuid().GetHashCode());
        var result = new char[length];
        for (int i = 0; i < length; i++)
        {
            result[i] = chars[random.Next(chars.Length)];
        }

        return new string(result);
    }

    private static string CalculateCheckDigits(string ibanWithoutChecksum)
    {
        var rearranged = ibanWithoutChecksum[4..] + ibanWithoutChecksum[..2] + "00";

        var numeric = string.Empty;
        foreach (var c in rearranged)
        {
            if (char.IsDigit(c))
            {
                numeric += c;
            }
            else if (char.IsLetter(c))
            {
                numeric += (c - 'A' + 10).ToString();
            }
        }

        var remainder = Mod97(numeric);
        var checksum = 98 - remainder;

        return checksum.ToString("D2");
    }

    private static int Mod97(string numericString)
    {
        return numericString.Aggregate(0, (current, digit) => (current * 10 + (digit - '0')) % 97);
    }

    private static bool IsValidChecksum(string iban)
    {
        if (iban.Length < 4)
            return false;

        var rearranged = iban.Substring(4) + iban.Substring(0, 4);
        var numeric = string.Empty;

        foreach (var c in rearranged)
        {
            if (char.IsDigit(c))
            {
                numeric += c;
            }
            else if (char.IsLetter(c))
            {
                numeric += (c - 'A' + 10).ToString();
            }
            else
            {
                return false;
            }
        }

        if (decimal.TryParse(numeric, out var number))
        {
            return number % 97 == 1;
        }

        return false;
    }

    public string ToFormattedString()
    {
        var formatted = string.Empty;
        for (var i = 0; i < Value.Length; i += 4)
        {
            var length = Math.Min(4, Value.Length - i);
            formatted += Value.Substring(i, length);
            if (i + length < Value.Length)
                formatted += " ";
        }

        return formatted;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as IBAN);
    }

    public bool Equals(IBAN? other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        return Value == other.Value;
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode(StringComparison.OrdinalIgnoreCase);
    }

    public static bool operator ==(IBAN? left, IBAN? right)
    {
        if (left is null && right is null)
            return true;

        if (left is null || right is null)
            return false;

        return left.Equals(right);
    }

    public static bool operator !=(IBAN? left, IBAN? right)
    {
        return !(left == right);
    }

    public override string ToString()
    {
        return ToFormattedString();
    }
}