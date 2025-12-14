using System.Text.RegularExpressions;

namespace Shared.Common.Logging;

public static class PiiMaskingExtensions
{
    private static readonly Regex IbanPattern = new(@"\b[A-Z]{2}\d{2}[A-Z0-9]{4,30}\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex EmailPattern = new(@"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b", RegexOptions.Compiled);
    private static readonly Regex CreditCardPattern = new(@"\b\d{4}[\s-]?\d{4}[\s-]?\d{4}[\s-]?\d{4}\b", RegexOptions.Compiled);

    public static string MaskIban(this string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        return IbanPattern.Replace(text, match =>
        {
            var iban = match.Value;
            return iban.Length <= 8 ? "****" : $"{iban[..4]}****{iban[^4..]}";
        });
    }

    public static string MaskEmail(this string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        return EmailPattern.Replace(text, match =>
        {
            var email = match.Value;
            var parts = email.Split('@');
            if (parts.Length != 2)
                return "***@***";

            var username = parts[0];
            var domain = parts[1];
            var maskedUsername = username.Length > 2
                ? $"{username[..2]}***"
                : "***";

            return $"{maskedUsername}@{domain}";
        });
    }

    public static string MaskCreditCard(this string text)
    {
        return string.IsNullOrEmpty(text) ? text :
            CreditCardPattern.Replace(text, "****-****-****-****");
    }

    public static string MaskPii(this string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        return text
            .MaskIban()
            .MaskEmail()
            .MaskCreditCard();
    }
}