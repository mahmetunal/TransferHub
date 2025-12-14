using MediatR;

namespace Account.Application.Commands.ReleaseReservation;

public sealed class ReleaseReservationCommand : IRequest<ReleaseReservationResult>
{
    public Guid TransferId { get; init; }
    public Guid ReservationId { get; init; }
}