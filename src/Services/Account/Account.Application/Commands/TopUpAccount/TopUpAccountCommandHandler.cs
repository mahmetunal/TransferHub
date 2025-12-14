using Account.Application.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Common.Persistence;
using Shared.Common.ValueObjects;

namespace Account.Application.Commands.TopUpAccount;

public sealed class TopUpAccountCommandHandler : IRequestHandler<TopUpAccountCommand, TopUpAccountResult>
{
    private readonly IAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TopUpAccountCommandHandler> _logger;

    public TopUpAccountCommandHandler(
        IAccountRepository accountRepository,
        IUnitOfWork unitOfWork,
        ILogger<TopUpAccountCommandHandler> logger)
    {
        _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<TopUpAccountResult> Handle(TopUpAccountCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "User {OwnerId} topping up account {Iban} with {Amount} {Currency}",
            request.OwnerId,
            request.Iban,
            request.Amount,
            request.Currency);

        try
        {
            var account = await _accountRepository.GetByIbanAsync(request.Iban, cancellationToken);

            if (account == null)
            {
                _logger.LogWarning("Account with IBAN {Iban} not found", request.Iban);
                throw new InvalidOperationException($"Account with IBAN {request.Iban} not found");
            }

            if (account.OwnerId != request.OwnerId)
                throw new UnauthorizedAccessException("You can only top up your own accounts");

            if (account.Balance.Currency.Code != request.Currency)
            {
                _logger.LogWarning(
                    "Currency mismatch: Account {Iban} has currency {AccountCurrency}, but top-up attempted with {TopUpCurrency}",
                    request.Iban,
                    account.Balance.Currency.Code,
                    request.Currency);

                throw new InvalidOperationException($"Currency mismatch. Account uses {account.Balance.Currency.Code}");
            }

            if (request.Amount <= 0)
                throw new InvalidOperationException("Top-up amount must be greater than zero");

            var currency = Currency.Create(request.Currency);
            var topUpMoney = Money.Create(request.Amount, currency);

            account.Credit(topUpMoney);

            await _accountRepository.UpdateAsync(account, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Account {Iban} topped up successfully. New balance: {NewBalance} {Currency}",
                request.Iban,
                account.Balance.Amount,
                account.Balance.Currency.Code);

            return new TopUpAccountResult
            {
                AccountId = account.Id,
                Iban = account.Iban.Value,
                NewBalance = account.Balance.Amount,
                Currency = account.Balance.Currency.Code,
                TopUpAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to top up account {Iban}",
                request.Iban);

            throw;
        }
    }
}