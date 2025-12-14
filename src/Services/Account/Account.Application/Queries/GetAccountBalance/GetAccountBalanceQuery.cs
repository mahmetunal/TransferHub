using MediatR;

namespace Account.Application.Queries.GetAccountBalance;

public sealed class GetAccountBalanceQuery : IRequest<GetAccountBalanceResult?>
{
    public string Iban { get; init; } = null!;
}