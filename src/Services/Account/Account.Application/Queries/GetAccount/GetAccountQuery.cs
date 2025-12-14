using Account.Application.DTOs;
using MediatR;

namespace Account.Application.Queries.GetAccount;

public sealed class GetAccountQuery : IRequest<AccountDto?>
{
    public string Iban { get; init; } = null!;
}