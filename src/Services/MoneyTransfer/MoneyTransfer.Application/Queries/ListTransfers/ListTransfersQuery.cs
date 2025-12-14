using System.Text.Json.Serialization;
using MediatR;

namespace MoneyTransfer.Application.Queries.ListTransfers;

public sealed class ListTransfersQuery : IRequest<ListTransfersResult>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? Status { get; init; }
    public string? SourceAccount { get; init; }
    public string? DestinationAccount { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }

    [JsonIgnore]
    public string? InitiatedBy { get; init; }
}