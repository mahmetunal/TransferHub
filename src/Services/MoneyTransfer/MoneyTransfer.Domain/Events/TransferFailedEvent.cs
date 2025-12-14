using Shared.Common.Events;

namespace MoneyTransfer.Domain.Events;

public sealed class TransferFailedEvent : IDomainEvent
{
    public Guid Id { get; }
    public DateTime OccurredOn { get; }
    public Guid TransferId { get; }
    public string Reason { get; }

    public TransferFailedEvent(Guid transferId, string reason, DateTime occurredOn)
    {
        Id = Guid.NewGuid();
        TransferId = transferId;
        Reason = reason;
        OccurredOn = occurredOn;
    }
}