using MassTransit;
using Microsoft.Extensions.Logging;
using MoneyTransfer.Application.Repositories;
using Shared.Common.Persistence;
using Shared.Common.Sagas.Events;

namespace MoneyTransfer.Infrastructure.Consumers;

/// <summary>
/// Consumer for TransferCancelled event from saga.
/// Marks transfer as cancelled.
/// </summary>
public sealed class TransferCancelledEventConsumer : IConsumer<TransferCancelledEvent>
{
    private readonly ITransferRepository _transferRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TransferCancelledEventConsumer> _logger;

    public TransferCancelledEventConsumer(
        ITransferRepository transferRepository,
        IUnitOfWork unitOfWork,
        ILogger<TransferCancelledEventConsumer> logger)
    {
        _transferRepository = transferRepository ?? throw new ArgumentNullException(nameof(transferRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Consume(ConsumeContext<TransferCancelledEvent> context)
    {
        _logger.LogInformation(
            "TransferCancelled event received for Transfer {TransferId}",
            context.Message.TransferId);

        var transfer = await _transferRepository.GetByIdWithEventsAsync(context.Message.TransferId, context.CancellationToken);
        if (transfer == null)
        {
            _logger.LogWarning("Transfer {TransferId} not found for TransferCancelled event", context.Message.TransferId);
            return;
        }

        try
        {
            // Truncate reason to 500 characters to match database constraint
            var reason = context.Message.Reason ?? "Transfer cancelled due to saga rollback";
            var truncatedReason = reason.Length > 500
                ? reason.Substring(0, 497) + "..."
                : reason;

            transfer.Cancel(truncatedReason);
            await _transferRepository.UpdateAsync(transfer, context.CancellationToken);
            await _unitOfWork.SaveChangesAsync(context.CancellationToken);

            _logger.LogInformation(
                "Transfer {TransferId} marked as Cancelled",
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