using Account.Application.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Common.Persistence;

namespace Account.Application.Commands.CommitReservation;

public sealed class CommitReservationCommandHandler : IRequestHandler<CommitReservationCommand, CommitReservationResult>
{
    private readonly IBalanceReservationRepository _reservationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CommitReservationCommandHandler> _logger;

    public CommitReservationCommandHandler(
        IBalanceReservationRepository reservationRepository,
        IAccountRepository accountRepository,
        IUnitOfWork unitOfWork,
        ILogger<CommitReservationCommandHandler> logger)
    {
        _reservationRepository = reservationRepository ?? throw new ArgumentNullException(nameof(reservationRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<CommitReservationResult> Handle(CommitReservationCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Committing reservation {ReservationId} for Transfer {TransferId}",
            request.ReservationId,
            request.TransferId);

        try
        {
            var reservation = await _reservationRepository.GetByIdAsync(request.ReservationId, cancellationToken);

            if (reservation == null)
                return new CommitReservationResult
                {
                    Success = false,
                    FailureReason = $"Reservation {request.ReservationId} not found"
                };

            reservation.Commit();

            await _reservationRepository.UpdateAsync(reservation, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Reservation committed successfully. ReservationId: {ReservationId}, TransferId: {TransferId}",
                request.ReservationId,
                request.TransferId);

            return new CommitReservationResult
            {
                Success = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to commit reservation {ReservationId} for Transfer {TransferId}",
                request.ReservationId,
                request.TransferId);

            return new CommitReservationResult
            {
                Success = false,
                FailureReason = ex.Message
            };
        }
    }
}