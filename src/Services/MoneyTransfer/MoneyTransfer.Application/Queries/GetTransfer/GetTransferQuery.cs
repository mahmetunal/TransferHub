using MediatR;
using MoneyTransfer.Application.DTOs;

namespace MoneyTransfer.Application.Queries.GetTransfer;

public sealed class GetTransferQuery : IRequest<TransferDto?>
{
    public Guid TransferId { get; init; }
    public string RequestedBy { get; init; } = null!;
}