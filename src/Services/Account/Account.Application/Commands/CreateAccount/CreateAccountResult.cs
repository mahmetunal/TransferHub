namespace Account.Application.Commands.CreateAccount;

public sealed class CreateAccountResult
{
    public Guid AccountId { get; init; }
    public string Iban { get; init; } = null!;
    public DateTime CreatedAt { get; init; }
}