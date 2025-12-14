using FluentValidation;

namespace Account.Application.Commands.ReleaseReservation;

public sealed class ReleaseReservationCommandValidator : AbstractValidator<ReleaseReservationCommand>
{
    public ReleaseReservationCommandValidator()
    {
        RuleFor(x => x.TransferId)
            .NotEmpty()
            .WithMessage("Transfer ID is required");

        RuleFor(x => x.ReservationId)
            .NotEmpty()
            .WithMessage("Reservation ID is required");
    }
}