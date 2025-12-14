using Shared.Common.Messaging;

namespace Shared.Common.Sagas.Events;

public sealed class TransferCancelledEvent : IDeduplicated
{
    public Guid TransferId { get; init; }
    public string? Reason { get; init; }

    public string DeduplicationKey => $"transfer-cancelled-{TransferId}";
}