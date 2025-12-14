using MediatR;

namespace Account.Application.Commands.CommitReservation;

public sealed class CommitReservationCommand : IRequest<CommitReservationResult>
{
    public Guid TransferId { get; init; }
    public Guid ReservationId { get; init; }
}