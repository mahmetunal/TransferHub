using Microsoft.AspNetCore.SignalR;
using MoneyTransfer.API.Hubs;
using MoneyTransfer.Application.DTOs;
using MoneyTransfer.Application.Services;

namespace MoneyTransfer.API.Services;

public sealed class TransferNotificationService : ITransferNotificationService
{
    private readonly IHubContext<TransferHub> _hubContext;
    private readonly ILogger<TransferNotificationService> _logger;

    public TransferNotificationService(
        IHubContext<TransferHub> hubContext,
        ILogger<TransferNotificationService> logger)
    {
        _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task NotifyTransferStatusChangedAsync(Guid transferId, TransferDto transfer, CancellationToken cancellationToken = default)
    {
        try
        {
            await _hubContext.Clients
                .Group($"transfer-{transferId}")
                .SendAsync("TransferStatusChanged", transfer, cancellationToken);

            _logger.LogInformation(
                "Sent transfer status notification for Transfer {TransferId}",
                transferId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to send transfer status notification for Transfer {TransferId}",
                transferId);
        }
    }
}