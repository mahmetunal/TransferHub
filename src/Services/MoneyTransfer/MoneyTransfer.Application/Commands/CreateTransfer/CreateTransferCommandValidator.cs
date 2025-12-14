using FluentValidation;

namespace MoneyTransfer.Application.Commands.CreateTransfer;

public sealed class CreateTransferCommandValidator : AbstractValidator<CreateTransferCommand>
{
    public CreateTransferCommandValidator()
    {
        RuleFor(x => x.SourceAccount)
            .NotEmpty()
            .WithMessage("Source account IBAN is required")
            .Must(BeValidIbanFormat)
            .WithMessage("Source account must be a valid IBAN format");

        RuleFor(x => x.DestinationAccount)
            .NotEmpty()
            .WithMessage("Destination account IBAN is required")
            .Must(BeValidIbanFormat)
            .WithMessage("Destination account must be a valid IBAN format")
            .NotEqual(x => x.SourceAccount)
            .WithMessage("Source and destination accounts cannot be the same");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .LessThanOrEqualTo(1_000_000);

        RuleFor(x => x.Currency)
            .NotEmpty()
            .Length(3)
            .Must(BeValidCurrency)
            .WithMessage("Currency must be a supported currency code (USD, EUR, GBP, TRY)");

        RuleFor(x => x.Reference)
            .MaximumLength(255)
            .WithMessage("Reference cannot exceed 255 characters")
            .When(x => !string.IsNullOrEmpty(x.Reference));
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