using Account.Application.Repositories;
using Account.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Common.Persistence;
using Shared.Common.ValueObjects;

namespace Account.Application.Commands.ReserveBalance;

public sealed class ReserveBalanceCommandHandler : IRequestHandler<ReserveBalanceCommand, ReserveBalanceResult>
{
    private readonly IAccountRepository _accountRepository;
    private readonly IBalanceReservationRepository _reservationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ReserveBalanceCommandHandler> _logger;

    public ReserveBalanceCommandHandler(
        IAccountRepository accountRepository,
        IBalanceReservationRepository reservationRepository,
        IUnitOfWork unitOfWork,
        ILogger<ReserveBalanceCommandHandler> logger)
    {
        _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
        _reservationRepository = reservationRepository ?? throw new ArgumentNullException(nameof(reservationRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ReserveBalanceResult> Handle(ReserveBalanceCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Reserving balance for Transfer {TransferId}, Account {AccountIban}, Amount {Amount} {Currency}, InitiatedBy {InitiatedBy}",
            request.TransferId,
            request.AccountIban,
            request.Amount,
            request.Currency,
            request.InitiatedBy);

        try
        {
            var account = await _accountRepository.GetByIbanWithReservationsAsync(request.AccountIban, cancellationToken);

            if (account == null)
            {
                _logger.LogWarning("Account not found: {AccountIban}", request.AccountIban);

                return new ReserveBalanceResult
                {
                    Success = false,
                    FailureReason = $"Account with IBAN {request.AccountIban} not found"
                };
            }

            if (account.OwnerId != request.InitiatedBy)
                return new ReserveBalanceResult
                {
                    Success = false,
                    FailureReason = "Unauthorized: You don't own the source account"
                };

            var currency = Currency.Create(request.Currency);
            var amount = Money.Create(request.Amount, currency);

            var reservationId = Guid.NewGuid();
            account.ReserveBalance(amount, reservationId);

            var reservation = BalanceReservation.Create(
                reservationId,
                account.Id,
                request.TransferId,
                amount);

            await _reservationRepository.AddAsync(reservation, cancellationToken);

            await _accountRepository.UpdateAsync(account, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Balance reserved successfully. ReservationId: {ReservationId}, TransferId: {TransferId}",
                reservationId,
                request.TransferId);

            return new ReserveBalanceResult
            {
                ReservationId = reservationId,
                Success = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to reserve balance for Transfer {TransferId}",
                request.TransferId);

            return new ReserveBalanceResult
            {
                Success = false,
                FailureReason = ex.Message
            };
        }
    }
}