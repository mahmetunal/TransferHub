using Shared.Common.Messaging;

namespace Shared.Common.Sagas.Commands;

public sealed class CommitReservationCommand : IDeduplicated
{
    public Guid TransferId { get; init; }
    public Guid ReservationId { get; init; }

    public string DeduplicationKey => $"commit-reservation-{TransferId}-{ReservationId}";
}