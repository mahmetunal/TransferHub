using MassTransit;
using Microsoft.Extensions.Logging;
using MoneyTransfer.Application.Repositories;
using MoneyTransfer.Domain.Entities;
using Shared.Common.Persistence;
using Shared.Common.Sagas.Events;

namespace MoneyTransfer.Infrastructure.Consumers;

/// <summary>
/// Consumer for BalanceReserved event from Account service.
/// Updates transfer status and forwards event to saga.
/// </summary>
public sealed class BalanceReservedEventConsumer : IConsumer<BalanceReservedEvent>
{
    private readonly ITransferRepository _transferRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<BalanceReservedEventConsumer> _logger;

    public BalanceReservedEventConsumer(
        ITransferRepository transferRepository,
        IUnitOfWork unitOfWork,
        ILogger<BalanceReservedEventConsumer> logger)
    {
        _transferRepository = transferRepository ?? throw new ArgumentNullException(nameof(transferRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Consume(ConsumeContext<BalanceReservedEvent> context)
    {
        _logger.LogInformation(
            "BalanceReserved event received for Transfer {TransferId}, Reservation {ReservationId}",
            context.Message.TransferId,
            context.Message.ReservationId);

        var transfer = await _transferRepository.GetByIdWithEventsAsync(context.Message.TransferId, context.CancellationToken);
        if (transfer == null)
        {
            _logger.LogWarning("Transfer {TransferId} not found for BalanceReserved event", context.Message.TransferId);
            return;
        }

        try
        {
            if (transfer.Status == TransferStatus.Pending)
            {
                transfer.MarkAsProcessing();
                await _transferRepository.UpdateAsync(transfer, context.CancellationToken);
                await _unitOfWork.SaveChangesAsync(context.CancellationToken);

                _logger.LogInformation(
                    "Transfer {TransferId} marked as Processing",
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