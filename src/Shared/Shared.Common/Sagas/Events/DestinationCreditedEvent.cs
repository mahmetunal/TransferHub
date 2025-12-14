using Shared.Common.Messaging;

namespace Shared.Common.Sagas.Events;

public sealed class DestinationCreditedEvent : IDeduplicated
{
    public Guid TransferId { get; init; }

    public string DeduplicationKey => $"destination-credited-{TransferId}";
}