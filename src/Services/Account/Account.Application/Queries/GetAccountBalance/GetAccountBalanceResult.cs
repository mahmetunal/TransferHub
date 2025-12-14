namespace Account.Application.Queries.GetAccountBalance;

public sealed class GetAccountBalanceResult
{
    public Guid AccountId { get; init; }
    public string Iban { get; init; } = null!;
    public decimal Balance { get; init; }
    public string Currency { get; init; } = null!;
    public bool IsActive { get; init; }
}