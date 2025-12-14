using Account.Application.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Common.Persistence;

namespace Account.Application.Commands.ReleaseReservation;

public sealed class ReleaseReservationCommandHandler : IRequestHandler<ReleaseReservationCommand, ReleaseReservationResult>
{
    private readonly IBalanceReservationRepository _reservationRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ReleaseReservationCommandHandler> _logger;

    public ReleaseReservationCommandHandler(
        IBalanceReservationRepository reservationRepository,
        IAccountRepository accountRepository,
        IUnitOfWork unitOfWork,
        ILogger<ReleaseReservationCommandHandler> logger)
    {
        _reservationRepository = reservationRepository ?? throw new ArgumentNullException(nameof(reservationRepository));
        _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<ReleaseReservationResult> Handle(ReleaseReservationCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Releasing reservation {ReservationId} for Transfer {TransferId}",
            request.ReservationId,
            request.TransferId);

        try
        {
            var reservation = await _reservationRepository.GetByIdAsync(request.ReservationId, cancellationToken);

            if (reservation == null)
                return new ReleaseReservationResult
                {
                    Success = false,
                    FailureReason = $"Reservation {request.ReservationId} not found"
                };

            var account = await _accountRepository.GetByIdAsync(reservation.AccountId, cancellationToken);

            if (account == null)
                return new ReleaseReservationResult
                {
                    Success = false,
                    FailureReason = $"Account {reservation.AccountId} not found"
                };

            account.ReleaseReservation(reservation.Amount, reservation.Id);
            reservation.Release();

            await _accountRepository.UpdateAsync(account, cancellationToken);

            await _reservationRepository.UpdateAsync(reservation, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Reservation released successfully. ReservationId: {ReservationId}, TransferId: {TransferId}",
                request.ReservationId,
                request.TransferId);

            return new ReleaseReservationResult
            {
                Success = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to release reservation {ReservationId} for Transfer {TransferId}",
                request.ReservationId,
                request.TransferId);

            return new ReleaseReservationResult
            {
                Success = false,
                FailureReason = ex.Message
            };
        }
    }
}