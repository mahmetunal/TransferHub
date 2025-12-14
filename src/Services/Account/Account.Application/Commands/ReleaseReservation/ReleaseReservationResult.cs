namespace Account.Application.Commands.ReleaseReservation;

public sealed class ReleaseReservationResult
{
    public bool Success { get; init; }
    public string? FailureReason { get; init; }
}