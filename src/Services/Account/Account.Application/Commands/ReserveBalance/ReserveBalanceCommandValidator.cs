using FluentValidation;

namespace Account.Application.Commands.ReserveBalance;

public sealed class ReserveBalanceCommandValidator : AbstractValidator<ReserveBalanceCommand>
{
    public ReserveBalanceCommandValidator()
    {
        RuleFor(x => x.TransferId)
            .NotEmpty()
            .WithMessage("Transfer ID is required");

        RuleFor(x => x.AccountIban)
            .NotEmpty()
            .WithMessage("Account IBAN is required")
            .Must(BeValidIbanFormat)
            .WithMessage("Account IBAN must be in valid format");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Amount must be greater than zero")
            .LessThanOrEqualTo(1_000_000)
            .WithMessage("Amount cannot exceed 1,000,000");

        RuleFor(x => x.Currency)
            .NotEmpty()
            .WithMessage("Currency is required")
            .Length(3)
            .WithMessage("Currency must be a valid ISO 4217 code (3 characters)")
            .Must(BeValidCurrency)
            .WithMessage("Currency must be a supported currency code (USD, EUR, GBP, TRY)");
    }

    private static bool BeValidIbanFormat(string? iban)
    {
        if (string.IsNullOrWhiteSpace(iban))
            return false;

        var normalized = iban.Replace(" ", "").Replace("-", "");
        return normalized.Length >= 15 && normalized.Length <= 34;
    }

    private static bool BeValidCurrency(string? currency)
    {
        if (string.IsNullOrWhiteSpace(currency))
            return false;

        var supportedCurrencies = new[] { "USD", "EUR", "GBP", "TRY" };
        return supportedCurrencies.Contains(currency.ToUpperInvariant());
    }
}