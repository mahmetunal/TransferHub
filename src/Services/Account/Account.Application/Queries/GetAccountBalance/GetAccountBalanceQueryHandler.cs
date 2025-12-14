using Account.Application.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Account.Application.Queries.GetAccountBalance;

public sealed class GetAccountBalanceQueryHandler : IRequestHandler<GetAccountBalanceQuery, GetAccountBalanceResult?>
{
    private readonly IAccountRepository _accountRepository;
    private readonly ILogger<GetAccountBalanceQueryHandler> _logger;

    public GetAccountBalanceQueryHandler(
        IAccountRepository accountRepository,
        ILogger<GetAccountBalanceQueryHandler> logger)
    {
        _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<GetAccountBalanceResult?> Handle(GetAccountBalanceQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving account balance for IBAN {@Request}", request);

        var account = await _accountRepository.GetByIbanAsync(request.Iban, cancellationToken);

        if (account == null)
        {
            _logger.LogWarning("Account with IBAN {@Iban} not found", request);

            return null;
        }

        return new GetAccountBalanceResult
        {
            AccountId = account.Id,
            Iban = account.Iban.Value,
            Balance = account.Balance.Amount,
            Currency = account.Balance.Currency.Code,
            IsActive = account.IsActive
        };
    }
}