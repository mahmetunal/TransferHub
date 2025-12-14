using Shared.Common.Events;

namespace MoneyTransfer.Domain.Events;

public sealed class TransferCreatedEvent : IDomainEvent
{
    public Guid Id { get; }
    public DateTime OccurredOn { get; }
    public Guid TransferId { get; }
    public string SourceAccount { get; }
    public string DestinationAccount { get; }
    public decimal Amount { get; }
    public string Currency { get; }

    public TransferCreatedEvent(
        Guid transferId,
        string sourceAccount,
        string destinationAccount,
        decimal amount,
        string currency,
        DateTime occurredOn)
    {
        Id = Guid.NewGuid();
        TransferId = transferId;
        SourceAccount = sourceAccount;
        DestinationAccount = destinationAccount;
        Amount = amount;
        Currency = currency;
        OccurredOn = occurredOn;
    }
}