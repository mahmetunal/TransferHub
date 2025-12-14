using System.Text.Json.Serialization;
using MediatR;

namespace Account.Application.Commands.TopUpAccount;

public sealed class TopUpAccountCommand : IRequest<TopUpAccountResult>
{
    [JsonIgnore]
    public string? Iban { get; init; }

    public decimal Amount { get; init; }
    public string Currency { get; init; } = null!;
    public string OwnerId { get; init; } = null!;
}