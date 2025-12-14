using Account.Application.DTOs;
using Account.Application.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Account.Application.Queries.GetAccount;

public sealed class GetAccountQueryHandler : IRequestHandler<GetAccountQuery, AccountDto?>
{
    private readonly IAccountRepository _accountRepository;
    private readonly ILogger<GetAccountQueryHandler> _logger;

    public GetAccountQueryHandler(
        IAccountRepository accountRepository,
        ILogger<GetAccountQueryHandler> logger)
    {
        _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<AccountDto?> Handle(GetAccountQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving account with IBAN {Iban}", request.Iban);

        var account = await _accountRepository.GetByIbanAsync(request.Iban, cancellationToken);

        if (account == null)
        {
            _logger.LogWarning("Account with IBAN {Iban} not found", request.Iban);

            return null;
        }

        return AccountDto.FromEntity(account);
    }
}