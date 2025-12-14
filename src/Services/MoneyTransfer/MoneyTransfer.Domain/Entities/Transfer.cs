using MoneyTransfer.Domain.Events;
using Shared.Common.Domain;
using Shared.Common.Domain.Contracts;
using Shared.Common.ValueObjects;

namespace MoneyTransfer.Domain.Entities;

public class Transfer : AuditableEntity, IAggregateRoot
{
    public IBAN SourceAccount { get; private set; } = null!;
    public IBAN DestinationAccount { get; private set; } = null!;
    public Money Amount { get; private set; } = null!;
    public TransferStatus Status { get; private set; }
    public string? FailureReason { get; private set; }
    public DateTime RequestedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public string? Reference { get; private set; }
    public string InitiatedBy { get; private set; } = null!;

    private Transfer()
    {
    }

    private Transfer(Guid id, IBAN sourceAccount, IBAN destinationAccount, Money amount, string initiatedBy, string? reference)
        : base(id)
    {
        ArgumentNullException.ThrowIfNull(sourceAccount);
        ArgumentNullException.ThrowIfNull(destinationAccount);
        ArgumentNullException.ThrowIfNull(amount);

        if (string.IsNullOrWhiteSpace(initiatedBy))
            throw new ArgumentException("InitiatedBy cannot be null or empty", nameof(initiatedBy));

        if (sourceAccount == destinationAccount)
            throw new InvalidOperationException("Source and destination accounts cannot be the same");

        SourceAccount = sourceAccount;
        DestinationAccount = destinationAccount;
        Amount = amount;
        InitiatedBy = initiatedBy;
        Status = TransferStatus.Pending;
        RequestedAt = DateTime.UtcNow;
        Reference = reference;

        RaiseDomainEvent(new TransferCreatedEvent(Id, SourceAccount.Value, DestinationAccount.Value, Amount.Amount, Amount.Currency.Code, RequestedAt));
    }

    public static Transfer Create(Guid id, IBAN sourceAccount, IBAN destinationAccount, Money amount, string initiatedBy, string? reference = null)
    {
        return new Transfer(id, sourceAccount, destinationAccount, amount, initiatedBy, reference);
    }

    public void MarkAsProcessing()
    {
        if (Status != TransferStatus.Pending)
            throw new InvalidOperationException($"Cannot mark transfer as processing. Current status: {Status}");

        Status = TransferStatus.Processing;

        RaiseDomainEvent(new TransferProcessingEvent(Id, DateTime.UtcNow));
    }

    public void MarkAsCompleted()
    {
        if (Status != TransferStatus.Processing)
            throw new InvalidOperationException($"Cannot mark transfer as completed. Current status: {Status}");

        Status = TransferStatus.Completed;
        CompletedAt = DateTime.UtcNow;

        RaiseDomainEvent(new TransferCompletedEvent(Id, SourceAccount.Value, DestinationAccount.Value, Amount.Amount, Amount.Currency.Code, CompletedAt.Value));
    }

    public void MarkAsFailed(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Failure reason cannot be null or empty", nameof(reason));

        if (Status == TransferStatus.Completed)
            throw new InvalidOperationException("Cannot mark completed transfer as failed");

        Status = TransferStatus.Failed;
        FailureReason = reason;

        RaiseDomainEvent(new TransferFailedEvent(Id, reason, DateTime.UtcNow));
    }

    public void Cancel(string? reason = null)
    {
        if (Status == TransferStatus.Completed)
            throw new InvalidOperationException("Cannot cancel completed transfer");

        if (Status == TransferStatus.Cancelled)
            return;

        Status = TransferStatus.Cancelled;
        FailureReason = reason;

        RaiseDomainEvent(new TransferCancelledEvent(Id, reason, DateTime.UtcNow));
    }
}