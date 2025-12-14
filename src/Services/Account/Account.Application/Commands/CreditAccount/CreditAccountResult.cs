namespace Account.Application.Commands.CreditAccount;

public sealed class CreditAccountResult
{
    public bool Success { get; init; }
    public string? FailureReason { get; init; }
}