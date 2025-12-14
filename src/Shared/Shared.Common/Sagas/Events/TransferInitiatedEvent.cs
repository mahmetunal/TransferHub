using Shared.Common.Messaging;

namespace Shared.Common.Sagas.Events;

public sealed class TransferInitiatedEvent : IDeduplicated
{
    public Guid TransferId { get; init; }
    public string SourceAccount { get; init; } = null!;
    public string DestinationAccount { get; init; } = null!;
    public decimal Amount { get; init; }
    public string Currency { get; init; } = null!;
    public string InitiatedBy { get; init; } = null!;

    public string DeduplicationKey => $"transfer-initiated-{TransferId}";
}