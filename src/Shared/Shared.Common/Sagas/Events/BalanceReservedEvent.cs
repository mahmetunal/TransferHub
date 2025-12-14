using Shared.Common.Messaging;

namespace Shared.Common.Sagas.Events;

public sealed class BalanceReservedEvent : IDeduplicated
{
    public Guid TransferId { get; init; }
    public Guid ReservationId { get; init; }

    public string DeduplicationKey => $"balance-reserved-{TransferId}-{ReservationId}";
}