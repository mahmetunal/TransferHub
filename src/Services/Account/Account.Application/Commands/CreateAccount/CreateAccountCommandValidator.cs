using FluentValidation;

namespace Account.Application.Commands.CreateAccount;

public sealed class CreateAccountCommandValidator : AbstractValidator<CreateAccountCommand>
{
    public CreateAccountCommandValidator()
    {
        RuleFor(x => x.InitialBalance)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Initial balance cannot be negative");

        RuleFor(x => x.Currency)
            .NotEmpty()
            .WithMessage("Currency is required")
            .Length(3)
            .WithMessage("Currency must be a valid ISO 4217 code (3 characters)")
            .Must(BeValidCurrency)
            .WithMessage("Currency must be a supported currency code (USD, EUR, GBP, TRY)");

        RuleFor(x => x.OwnerId)
            .NotEmpty()
            .WithMessage("Owner ID is required")
            .MaximumLength(100)
            .WithMessage("Owner ID cannot exceed 100 characters");
    }

    private static bool BeValidCurrency(string? currency)
    {
        if (string.IsNullOrWhiteSpace(currency))
            return false;

        var supportedCurrencies = new[] { "USD", "EUR", "GBP", "TRY" };
        return supportedCurrencies.Contains(currency.ToUpperInvariant());
    }
}