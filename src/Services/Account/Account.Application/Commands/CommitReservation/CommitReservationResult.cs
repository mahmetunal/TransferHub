namespace Account.Application.Commands.CommitReservation;

public sealed class CommitReservationResult
{
    public bool Success { get; init; }
    public string? FailureReason { get; init; }
}