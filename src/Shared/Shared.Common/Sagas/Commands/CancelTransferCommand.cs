using Shared.Common.Messaging;

namespace Shared.Common.Sagas.Commands;

public sealed class CancelTransferCommand : IDeduplicated
{
    public Guid TransferId { get; init; }
    public string? Reason { get; init; }

    public string DeduplicationKey => $"cancel-transfer-{TransferId}";
}