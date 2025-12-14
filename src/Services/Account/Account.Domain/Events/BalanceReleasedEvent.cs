using Shared.Common.Events;

namespace Account.Domain.Events;

public sealed class BalanceReleasedEvent : IDomainEvent
{
    public Guid Id { get; }
    public DateTime OccurredOn { get; }
    public Guid AccountId { get; }
    public Guid ReservationId { get; }
    public decimal Amount { get; }
    public string Currency { get; }
    public decimal NewBalance { get; }

    public BalanceReleasedEvent(
        Guid accountId,
        Guid reservationId,
        decimal amount,
        string currency,
        decimal newBalance,
        DateTime occurredOn)
    {
        Id = Guid.NewGuid();
        AccountId = accountId;
        ReservationId = reservationId;
        Amount = amount;
        Currency = currency;
        NewBalance = newBalance;
        OccurredOn = occurredOn;
    }
}