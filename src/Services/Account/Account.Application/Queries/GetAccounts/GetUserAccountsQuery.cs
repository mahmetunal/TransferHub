using Account.Application.DTOs;
using MediatR;

namespace Account.Application.Queries.GetAccounts;

public record GetUserAccountsQuery(
    string OwnerId) : IRequest<List<AccountDto>>;