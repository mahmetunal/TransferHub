using Shared.Common.Events;

namespace MoneyTransfer.Domain.Events;

public sealed class TransferCancelledEvent : IDomainEvent
{
    public Guid Id { get; }
    public DateTime OccurredOn { get; }
    public Guid TransferId { get; }
    public string? Reason { get; }

    public TransferCancelledEvent(Guid transferId, string? reason, DateTime occurredOn)
    {
        Id = Guid.NewGuid();
        TransferId = transferId;
        Reason = reason;
        OccurredOn = occurredOn;
    }
}