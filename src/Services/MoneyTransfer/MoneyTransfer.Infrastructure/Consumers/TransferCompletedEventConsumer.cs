using MassTransit;
using Microsoft.Extensions.Logging;
using MoneyTransfer.Application.DTOs;
using MoneyTransfer.Application.Repositories;
using MoneyTransfer.Application.Services;
using MoneyTransfer.Domain.Entities;
using Shared.Common.Persistence;
using Shared.Common.Sagas.Events;

namespace MoneyTransfer.Infrastructure.Consumers;

/// <summary>
/// Consumer for TransferCompleted event from saga.
/// Marks transfer as completed.
/// </summary>
public sealed class TransferCompletedEventConsumer : IConsumer<TransferCompletedEvent>
{
    private readonly ITransferRepository _transferRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITransferNotificationService _notificationService;
    private readonly ILogger<TransferCompletedEventConsumer> _logger;

    public TransferCompletedEventConsumer(
        ITransferRepository transferRepository,
        IUnitOfWork unitOfWork,
        ITransferNotificationService notificationService,
        ILogger<TransferCompletedEventConsumer> logger)
    {
        _transferRepository = transferRepository ?? throw new ArgumentNullException(nameof(transferRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Consume(ConsumeContext<TransferCompletedEvent> context)
    {
        _logger.LogInformation(
            "TransferCompleted event received for Transfer {TransferId}",
            context.Message.TransferId);

        var transfer = await _transferRepository.GetByIdWithEventsAsync(context.Message.TransferId, context.CancellationToken);
        if (transfer == null)
        {
            _logger.LogWarning("Transfer {TransferId} not found for TransferCompleted event", context.Message.TransferId);
            return;
        }

        try
        {
            if (transfer.Status == TransferStatus.Processing)
            {
                transfer.MarkAsCompleted();
                await _transferRepository.UpdateAsync(transfer, context.CancellationToken);
                await _unitOfWork.SaveChangesAsync(context.CancellationToken);

                // Send real-time notification
                var transferDto = new TransferDto
                {
                    Id = transfer.Id,
                    SourceAccount = transfer.SourceAccount.Value,
                    DestinationAccount = transfer.DestinationAccount.Value,
                    Amount = transfer.Amount.Amount,
                    Currency = transfer.Amount.Currency.Code,
                    Status = transfer.Status.ToString(),
                    FailureReason = transfer.FailureReason,
                    Reference = transfer.Reference,
                    CreatedAt = transfer.RequestedAt,
                    CompletedAt = transfer.CompletedAt
                };

                await _notificationService.NotifyTransferStatusChangedAsync(transfer.Id, transferDto, context.CancellationToken);

                _logger.LogInformation(
                    "Transfer {TransferId} marked as Completed",
                    context.Message.TransferId);
            }
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