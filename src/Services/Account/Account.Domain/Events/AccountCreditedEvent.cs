using Shared.Common.Events;

namespace Account.Domain.Events;

public sealed class AccountCreditedEvent : IDomainEvent
{
    public Guid Id { get; }
    public DateTime OccurredOn { get; }
    public Guid AccountId { get; }
    public decimal Amount { get; }
    public string Currency { get; }
    public decimal NewBalance { get; }

    public AccountCreditedEvent(
        Guid accountId,
        decimal amount,
        string currency,
        decimal newBalance,
        DateTime occurredOn)
    {
        Id = Guid.NewGuid();
        AccountId = accountId;
        Amount = amount;
        Currency = currency;
        NewBalance = newBalance;
        OccurredOn = occurredOn;
    }
}