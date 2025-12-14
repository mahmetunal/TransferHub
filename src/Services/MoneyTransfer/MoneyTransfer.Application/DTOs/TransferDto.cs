namespace MoneyTransfer.Application.DTOs;

public sealed class TransferDto
{
    public Guid Id { get; init; }
    public string SourceAccount { get; init; } = null!;
    public string DestinationAccount { get; init; } = null!;
    public decimal Amount { get; init; }
    public string Currency { get; init; } = null!;
    public string Status { get; init; } = null!;
    public string? FailureReason { get; init; }
    public string? Reference { get; init; }
    public string InitiatedBy { get; init; } = null!;
    public DateTime CreatedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
}