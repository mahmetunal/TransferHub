using Shared.Common.Messaging;

namespace Shared.Common.Sagas.Commands;

public sealed class ReserveBalanceCommand : IDeduplicated
{
    public Guid TransferId { get; init; }
    public string AccountIban { get; init; } = null!;
    public decimal Amount { get; init; }
    public string Currency { get; init; } = null!;
    public string InitiatedBy { get; init; } = null!;

    public string DeduplicationKey => $"reserve-balance-{TransferId}";
}