using Shared.Common.Messaging;

namespace Shared.Common.Sagas.Events;

public sealed class TransferCompletedEvent : IDeduplicated
{
    public Guid TransferId { get; init; }

    public string DeduplicationKey => $"transfer-completed-{TransferId}";
}