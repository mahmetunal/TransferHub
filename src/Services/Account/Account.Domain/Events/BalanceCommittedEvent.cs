using Shared.Common.Events;

namespace Account.Domain.Events;

public sealed class BalanceCommittedEvent : IDomainEvent
{
    public Guid Id { get; }
    public DateTime OccurredOn { get; }
    public Guid AccountId { get; }
    public Guid ReservationId { get; }
    public decimal Amount { get; }
    public string Currency { get; }
    public decimal RemainingBalance { get; }

    public BalanceCommittedEvent(
        Guid accountId,
        Guid reservationId,
        decimal amount,
        string currency,
        decimal remainingBalance,
        DateTime occurredOn)
    {
        Id = Guid.NewGuid();
        AccountId = accountId;
        ReservationId = reservationId;
        Amount = amount;
        Currency = currency;
        RemainingBalance = remainingBalance;
        OccurredOn = occurredOn;
    }
}