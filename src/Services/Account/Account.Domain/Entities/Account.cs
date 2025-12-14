using Account.Domain.Events;
using Shared.Common.Domain;
using Shared.Common.Domain.Contracts;
using Shared.Common.ValueObjects;

namespace Account.Domain.Entities;

public class Account : AuditableEntity, IAggregateRoot
{
    public IBAN Iban { get; private set; } = null!;
    public Money Balance { get; private set; } = null!;
    public string OwnerId { get; private set; } = null!;
    public DateTime? UpdatedAt { get; private set; }
    public bool IsActive { get; private set; }

    private Account()
    {
    }

    private Account(Guid id, IBAN iban, Money initialBalance, string ownerId)
        : base(id)
    {
        ArgumentNullException.ThrowIfNull(iban);

        ArgumentNullException.ThrowIfNull(initialBalance);

        if (initialBalance.Amount < 0)
            throw new ArgumentException("Initial balance cannot be negative", nameof(initialBalance));

        if (string.IsNullOrWhiteSpace(ownerId))
            throw new ArgumentException("Owner ID cannot be null or empty", nameof(ownerId));

        Iban = iban;
        Balance = initialBalance;
        OwnerId = ownerId;
        IsActive = true;

        RaiseDomainEvent(new AccountCreatedEvent(Id, Iban.Value, Balance.Amount, Balance.Currency.Code, OwnerId, CreatedAt.DateTime));
    }

    public static Account Create(Guid id, IBAN iban, Money initialBalance, string ownerId)
    {
        return new Account(id, iban, initialBalance, ownerId);
    }

    public void ReserveBalance(Money amount, Guid reservationId)
    {
        ArgumentNullException.ThrowIfNull(amount);

        if (amount.Amount <= 0)
            throw new ArgumentException("Reservation amount must be greater than zero", nameof(amount));

        if (Balance.Currency != amount.Currency)
            throw new InvalidOperationException($"Cannot reserve balance. Currency mismatch. Account: {Balance.Currency.Code}, Requested: {amount.Currency.Code}");

        if (!IsActive)
            throw new InvalidOperationException("Cannot reserve balance on inactive account");

        if (Balance.IsLessThan(amount))
            throw new InvalidOperationException($"Insufficient balance. Available: {Balance}, Requested: {amount}");

        Balance = Balance.Subtract(amount);

        RaiseDomainEvent(new BalanceReservedEvent(Id, reservationId, amount.Amount, amount.Currency.Code, Balance.Amount, DateTime.UtcNow));
    }

    public void ReleaseReservation(Money amount, Guid reservationId)
    {
        ArgumentNullException.ThrowIfNull(amount);

        if (amount.Amount <= 0)
            throw new ArgumentException("Release amount must be greater than zero", nameof(amount));

        if (Balance.Currency != amount.Currency)
            throw new InvalidOperationException($"Cannot release reservation. Currency mismatch. Account: {Balance.Currency.Code}, Requested: {amount.Currency.Code}");

        Balance = Balance.Add(amount);

        RaiseDomainEvent(new BalanceReleasedEvent(Id, reservationId, amount.Amount, amount.Currency.Code, Balance.Amount, DateTime.UtcNow));
    }

    public void CommitReservation(Money amount, Guid reservationId)
    {
        ArgumentNullException.ThrowIfNull(amount);

        if (amount.Amount <= 0)
            throw new ArgumentException("Commit amount must be greater than zero", nameof(amount));

        if (Balance.Currency != amount.Currency)
            throw new InvalidOperationException($"Cannot commit reservation. Currency mismatch. Account: {Balance.Currency.Code}, Requested: {amount.Currency.Code}");

        RaiseDomainEvent(new BalanceCommittedEvent(Id, reservationId, amount.Amount, amount.Currency.Code, Balance.Amount, DateTime.UtcNow));
    }

    public void Credit(Money amount)
    {
        ArgumentNullException.ThrowIfNull(amount);

        if (amount.Amount <= 0)
            throw new ArgumentException("Credit amount must be greater than zero", nameof(amount));

        if (Balance.Currency != amount.Currency)
            throw new InvalidOperationException($"Cannot credit account. Currency mismatch. Account: {Balance.Currency.Code}, Requested: {amount.Currency.Code}");

        if (!IsActive)
            throw new InvalidOperationException("Cannot credit inactive account");

        Balance = Balance.Add(amount);

        RaiseDomainEvent(new AccountCreditedEvent(Id, amount.Amount, amount.Currency.Code, Balance.Amount, DateTime.UtcNow));
    }

    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;

        RaiseDomainEvent(new AccountDeactivatedEvent(Id, DateTime.UtcNow));
    }

    public void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;

        RaiseDomainEvent(new AccountActivatedEvent(Id, DateTime.UtcNow));
    }
}