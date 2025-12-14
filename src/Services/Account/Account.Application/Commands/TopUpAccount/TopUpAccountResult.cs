namespace Account.Application.Commands.TopUpAccount;

public sealed class TopUpAccountResult
{
    public Guid AccountId { get; init; }
    public string Iban { get; init; } = null!;
    public decimal NewBalance { get; init; }
    public string Currency { get; init; } = null!;
    public DateTime TopUpAt { get; init; }
}