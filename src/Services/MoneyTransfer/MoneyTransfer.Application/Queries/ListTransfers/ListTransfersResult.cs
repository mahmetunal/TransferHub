using MoneyTransfer.Application.DTOs;

namespace MoneyTransfer.Application.Queries.ListTransfers;

public sealed class ListTransfersResult
{
    public List<TransferDto> Transfers { get; init; } = new();
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => (int) Math.Ceiling(TotalCount / (double) PageSize);
}