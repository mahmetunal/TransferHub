using FluentValidation;

namespace Account.Application.Commands.CommitReservation;

public sealed class CommitReservationCommandValidator : AbstractValidator<CommitReservationCommand>
{
    public CommitReservationCommandValidator()
    {
        RuleFor(x => x.TransferId)
            .NotEmpty()
            .WithMessage("Transfer ID is required");

        RuleFor(x => x.ReservationId)
            .NotEmpty()
            .WithMessage("Reservation ID is required");
    }
}