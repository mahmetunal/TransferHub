using Shared.Common.Messaging;

namespace Shared.Common.Sagas.Commands;

public sealed class CreditAccountCommand : IDeduplicated
{
    public Guid TransferId { get; init; }
    public string AccountIban { get; init; } = null!;
    public decimal Amount { get; init; }
    public string Currency { get; init; } = null!;

    public string DeduplicationKey => $"credit-account-{TransferId}";
}