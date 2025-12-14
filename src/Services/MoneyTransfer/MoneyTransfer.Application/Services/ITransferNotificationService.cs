using MoneyTransfer.Application.DTOs;

namespace MoneyTransfer.Application.Services;

public interface ITransferNotificationService
{
    Task NotifyTransferStatusChangedAsync(Guid transferId, TransferDto transfer, CancellationToken cancellationToken = default);
}