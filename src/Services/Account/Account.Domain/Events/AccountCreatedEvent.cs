using Shared.Common.Events;

namespace Account.Domain.Events;

public sealed class AccountCreatedEvent : IDomainEvent
{
    public Guid Id { get; }
    public DateTime OccurredOn { get; }
    public Guid AccountId { get; }
    public string Iban { get; }
    public decimal InitialBalance { get; }
    public string Currency { get; }
    public string OwnerId { get; }

    public AccountCreatedEvent(
        Guid accountId,
        string iban,
        decimal initialBalance,
        string currency,
        string ownerId,
        DateTime occurredOn)
    {
        Id = Guid.NewGuid();
        AccountId = accountId;
        Iban = iban;
        InitialBalance = initialBalance;
        Currency = currency;
        OwnerId = ownerId;
        OccurredOn = occurredOn;
    }
}