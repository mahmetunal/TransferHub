using Account.Application.DTOs;
using Account.Application.Repositories;
using MediatR;

namespace Account.Application.Queries.GetAccounts;

public class GetUserAccountsQueryHandler : IRequestHandler<GetUserAccountsQuery, List<AccountDto>>
{
    private readonly IAccountRepository _accountRepository;

    public GetUserAccountsQueryHandler(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository;
    }

    public async Task<List<AccountDto>> Handle(GetUserAccountsQuery request, CancellationToken cancellationToken)
    {
        var accounts = await _accountRepository.GetByOwnerIdAsync(request.OwnerId, cancellationToken);

        return accounts.Select(AccountDto.FromEntity).ToList();
    }
}