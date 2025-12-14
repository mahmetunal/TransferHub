using Account.Application.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Common.Persistence;
using Shared.Common.ValueObjects;

namespace Account.Application.Commands.CreditAccount;

public sealed class CreditAccountCommandHandler : IRequestHandler<CreditAccountCommand, CreditAccountResult>
{
    private readonly IAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreditAccountCommandHandler> _logger;

    public CreditAccountCommandHandler(
        IAccountRepository accountRepository,
        IUnitOfWork unitOfWork,
        ILogger<CreditAccountCommandHandler> logger)
    {
        _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<CreditAccountResult> Handle(CreditAccountCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Crediting account for Transfer {TransferId}, Account {AccountIban}, Amount {Amount} {Currency}",
            request.TransferId,
            request.AccountIban,
            request.Amount,
            request.Currency);

        try
        {
            var account = await _accountRepository.GetByIbanAsync(request.AccountIban, cancellationToken);

            if (account == null)
                return new CreditAccountResult
                {
                    Success = false,
                    FailureReason = $"Account with IBAN {request.AccountIban} not found"
                };

            var currency = Currency.Create(request.Currency);
            var amount = Money.Create(request.Amount, currency);

            account.Credit(amount);

            await _accountRepository.UpdateAsync(account, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Account credited successfully. TransferId: {TransferId}",
                request.TransferId);

            return new CreditAccountResult
            {
                Success = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to credit account for Transfer {TransferId}",
                request.TransferId);

            return new CreditAccountResult
            {
                Success = false,
                FailureReason = ex.Message
            };
        }
    }
}