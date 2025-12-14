using FluentValidation;

namespace Account.Application.Commands.TopUpAccount;

public sealed class TopUpAccountCommandValidator : AbstractValidator<TopUpAccountCommand>
{
    public TopUpAccountCommandValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0);

        RuleFor(x => x.Currency)
            .NotEmpty()
            .Length(3);

        RuleFor(x => x.OwnerId)
            .NotEmpty();
    }
}