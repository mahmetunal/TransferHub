using Shared.Common.Messaging;

namespace Shared.Common.Sagas.Commands;

public sealed class ReleaseReservationCommand : IDeduplicated
{
    public Guid TransferId { get; init; }
    public Guid ReservationId { get; init; }

    public string DeduplicationKey => $"release-reservation-{TransferId}-{ReservationId}";
}