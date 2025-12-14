namespace MoneyTransfer.Application.Commands.CreateTransfer;

public sealed class CreateTransferResult
{
    public Guid TransferId { get; init; }
    public string Status { get; init; } = null!;
    public DateTime CreatedAt { get; init; }
}