using MassTransit;
using Microsoft.Extensions.Logging;
using MoneyTransfer.Application.Repositories;
using Shared.Common.Persistence;
using Shared.Common.Sagas.Events;

namespace MoneyTransfer.Infrastructure.Consumers;

/// <summary>
/// Consumer for BalanceReservationFailed event from Account service.
/// Marks transfer as failed.
/// </summary>
public sealed class BalanceReservationFailedEventConsumer : IConsumer<BalanceReservationFailedEvent>
{
    private readonly ITransferRepository _transferRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<BalanceReservationFailedEventConsumer> _logger;

    public BalanceReservationFailedEventConsumer(
        ITransferRepository transferRepository,
        IUnitOfWork unitOfWork,
        ILogger<BalanceReservationFailedEventConsumer> logger)
    {
        _transferRepository = transferRepository ?? throw new ArgumentNullException(nameof(transferRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Consume(ConsumeContext<BalanceReservationFailedEvent> context)
    {
        _logger.LogInformation(
            "BalanceReservationFailed event received for Transfer {TransferId}, Reason: {Reason}",
            context.Message.TransferId,
            context.Message.Reason);

        var transfer = await _transferRepository.GetByIdWithEventsAsync(context.Message.TransferId, context.CancellationToken);
        if (transfer == null)
        {
            _logger.LogWarning("Transfer {TransferId} not found for BalanceReservationFailed event", context.Message.TransferId);
            return;
        }

        try
        {
            // Truncate reason to 500 characters to match database constraint
            var truncatedReason = context.Message.Reason?.Length > 500
                ? context.Message.Reason.Substring(0, 497) + "..."
                : context.Message.Reason ?? "Unknown error";

            transfer.MarkAsFailed(truncatedReason);
            await _transferRepository.UpdateAsync(transfer, context.CancellationToken);
            await _unitOfWork.SaveChangesAsync(context.CancellationToken);

            _logger.LogInformation(
                "Transfer {TransferId} marked as Failed",
                context.Message.TransferId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to update transfer {TransferId} status",
                context.Message.TransferId);
            throw;
        }
    }
}