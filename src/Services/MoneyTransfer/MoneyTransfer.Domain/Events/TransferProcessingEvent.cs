using Shared.Common.Events;

namespace MoneyTransfer.Domain.Events;

public sealed class TransferProcessingEvent : IDomainEvent
{
    public Guid Id { get; }
    public DateTime OccurredOn { get; }
    public Guid TransferId { get; }

    public TransferProcessingEvent(Guid transferId, DateTime occurredOn)
    {
        Id = Guid.NewGuid();
        TransferId = transferId;
        OccurredOn = occurredOn;
    }
}