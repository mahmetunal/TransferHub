using Account.Application.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Common.Persistence;
using Shared.Common.ValueObjects;
using AccountEntity = Account.Domain.Entities.Account;

namespace Account.Application.Commands.CreateAccount;

public sealed class CreateAccountCommandHandler : IRequestHandler<CreateAccountCommand, CreateAccountResult>
{
    private readonly IAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateAccountCommandHandler> _logger;

    public CreateAccountCommandHandler(
        IAccountRepository accountRepository,
        IUnitOfWork unitOfWork,
        ILogger<CreateAccountCommandHandler> logger)
    {
        _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<CreateAccountResult> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Creating account for owner {OwnerId} with initial balance {Balance} {Currency}",
            request.OwnerId,
            request.InitialBalance,
            request.Currency);

        try
        {
            IBAN iban;
            const int maxAttempts = 10;
            var attempt = 0;

            do
            {
                iban = IBAN.Generate();
                var existingAccount = await _accountRepository.GetByIbanAsync(iban.Value, cancellationToken);

                if (existingAccount == null)
                    break;

                attempt++;
                _logger.LogWarning("Generated IBAN {Iban} already exists, retrying... (attempt {Attempt})", iban.Value, attempt);
            } while (attempt < maxAttempts);

            if (attempt >= maxAttempts)
            {
                throw new InvalidOperationException("Failed to generate unique IBAN after multiple attempts");
            }

            var currency = Currency.Create(request.Currency);
            var initialBalance = Money.Create(request.InitialBalance, currency);

            var account = AccountEntity.Create(
                Guid.NewGuid(),
                iban,
                initialBalance,
                request.OwnerId);

            await _accountRepository.AddAsync(account, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Account {AccountId} created successfully with auto-generated IBAN {Iban} for owner {OwnerId}",
                account.Id,
                account.Iban.Value,
                request.OwnerId);

            return new CreateAccountResult
            {
                AccountId = account.Id,
                Iban = account.Iban.Value,
                CreatedAt = account.CreatedAt.DateTime
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to create account for owner {OwnerId}",
                request.OwnerId);

            throw;
        }
    }
}