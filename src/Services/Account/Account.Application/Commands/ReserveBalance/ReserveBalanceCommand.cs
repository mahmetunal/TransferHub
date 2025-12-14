using MediatR;

namespace Account.Application.Commands.ReserveBalance;

public sealed class ReserveBalanceCommand : IRequest<ReserveBalanceResult>
{
    public Guid TransferId { get; init; }
    public string AccountIban { get; init; } = null!;
    public decimal Amount { get; init; }
    public string Currency { get; init; } = null!;
    public string InitiatedBy { get; init; } = null!;
}