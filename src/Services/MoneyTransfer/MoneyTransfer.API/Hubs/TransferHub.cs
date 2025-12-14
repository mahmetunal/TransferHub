using Microsoft.AspNetCore.SignalR;

namespace MoneyTransfer.API.Hubs;

public sealed class TransferHub : Hub
{
    public async Task JoinTransferGroup(string transferId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"transfer-{transferId}");
    }

    public async Task LeaveTransferGroup(string transferId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"transfer-{transferId}");
    }
}