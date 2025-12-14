using System.Text.Json.Serialization;
using MediatR;

namespace Account.Application.Commands.CreateAccount;

public sealed class CreateAccountCommand : IRequest<CreateAccountResult>
{
    public decimal InitialBalance { get; init; }
    public string Currency { get; init; } = null!;

    [JsonIgnore]
    public string? OwnerId { get; init; }
}