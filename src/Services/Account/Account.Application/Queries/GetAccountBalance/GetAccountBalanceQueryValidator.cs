using FluentValidation;

namespace Account.Application.Queries.GetAccountBalance;

public sealed class GetAccountBalanceQueryValidator : AbstractValidator<GetAccountBalanceQuery>
{
    public GetAccountBalanceQueryValidator()
    {
        RuleFor(x => x.Iban)
            .NotEmpty()
            .WithMessage("IBAN is required")
            .Must(BeValidIbanFormat)
            .WithMessage("IBAN must be in valid format");
    }

    private static bool BeValidIbanFormat(string? iban)
    {
        if (string.IsNullOrWhiteSpace(iban))
            return false;

        var normalized = iban.Replace(" ", "").Replace("-", "");
        return normalized.Length >= 15 && normalized.Length <= 34;
    }
}