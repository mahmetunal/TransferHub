using System.Text.Json.Serialization;
using MediatR;

namespace MoneyTransfer.Application.Commands.CreateTransfer;

public sealed class CreateTransferCommand : IRequest<CreateTransferResult>
{
    public string SourceAccount { get; init; } = null!;
    public string DestinationAccount { get; init; } = null!;
    public decimal Amount { get; init; }
    public string Currency { get; init; } = null!;
    public string? Reference { get; init; }

    [JsonIgnore]
    public string? InitiatedBy { get; init; }
}