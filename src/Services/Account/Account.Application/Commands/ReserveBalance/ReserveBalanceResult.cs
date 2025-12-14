namespace Account.Application.Commands.ReserveBalance;

public sealed class ReserveBalanceResult
{
    public Guid ReservationId { get; init; }
    public bool Success { get; init; }
    public string? FailureReason { get; init; }
}