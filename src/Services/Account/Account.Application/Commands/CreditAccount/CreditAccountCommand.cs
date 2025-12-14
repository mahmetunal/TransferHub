using MediatR;

namespace Account.Application.Commands.CreditAccount;

public sealed class CreditAccountCommand : IRequest<CreditAccountResult>
{
    public Guid TransferId { get; init; }
    public string AccountIban { get; init; } = null!;
    public decimal Amount { get; init; }
    public string Currency { get; init; } = null!;
}