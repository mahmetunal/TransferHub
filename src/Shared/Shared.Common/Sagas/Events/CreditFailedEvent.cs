using Shared.Common.Messaging;

namespace Shared.Common.Sagas.Events;

public sealed class CreditFailedEvent : IDeduplicated
{
    public Guid TransferId { get; init; }
    public string Reason { get; init; } = null!;

    public string DeduplicationKey => $"credit-failed-{TransferId}";
}