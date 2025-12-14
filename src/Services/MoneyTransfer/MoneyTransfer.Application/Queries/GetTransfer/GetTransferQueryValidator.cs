using FluentValidation;

namespace MoneyTransfer.Application.Queries.GetTransfer;

public sealed class GetTransferQueryValidator : AbstractValidator<GetTransferQuery>
{
    public GetTransferQueryValidator()
    {
        RuleFor(x => x.TransferId)
            .NotEmpty()
            .WithMessage("Transfer ID is required");
    }
}