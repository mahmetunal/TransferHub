namespace Account.Application.DTOs;

public sealed class AccountDto
{
    public Guid Id { get; init; }
    public string Iban { get; init; } = null!;
    public decimal Balance { get; init; }
    public string Currency { get; init; } = null!;
    public string OwnerId { get; init; } = null!;
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }

    public static AccountDto FromEntity(Domain.Entities.Account account)
    {
        return new AccountDto
        {
            Id = account.Id,
            Iban = account.Iban.Value,
            Balance = account.Balance.Amount,
            Currency = account.Balance.Currency.Code,
            OwnerId = account.OwnerId,
            IsActive = account.IsActive,
            CreatedAt = account.CreatedAt.DateTime,
            UpdatedAt = account.UpdatedAt
        };
    }
}