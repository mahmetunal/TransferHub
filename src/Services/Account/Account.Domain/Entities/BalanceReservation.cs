using Shared.Common.Domain;
using Shared.Common.ValueObjects;

namespace Account.Domain.Entities;

public class BalanceReservation : BaseEntity
{
    public Guid AccountId { get; private set; }
    public Guid TransferId { get; private set; }
    public Money Amount { get; private set; } = null!;
    public DateTime ReservedAt { get; private set; }
    public DateTime? CommittedAt { get; private set; }
    public DateTime? ReleasedAt { get; private set; }
    public bool IsCommitted => CommittedAt.HasValue;
    public bool IsReleased => ReleasedAt.HasValue;
    public bool IsActive => !IsCommitted && !IsReleased;

    private BalanceReservation()
    {
    }

    private BalanceReservation(Guid id, Guid accountId, Guid transferId, Money amount)
        : base(id)
    {
        if (accountId == Guid.Empty)
            throw new ArgumentException("Account ID cannot be empty", nameof(accountId));

        if (transferId == Guid.Empty)
            throw new ArgumentException("Transfer ID cannot be empty", nameof(transferId));

        ArgumentNullException.ThrowIfNull(amount);

        if (amount.Amount <= 0)
            throw new ArgumentException("Reservation amount must be greater than zero", nameof(amount));

        AccountId = accountId;
        TransferId = transferId;
        Amount = amount;
        ReservedAt = DateTime.UtcNow;
    }

    public static BalanceReservation Create(Guid id, Guid accountId, Guid transferId, Money amount)
    {
        return new BalanceReservation(id, accountId, transferId, amount);
    }

    public void Commit()
    {
        if (IsCommitted)
            throw new InvalidOperationException("Reservation is already committed");

        if (IsReleased)
            throw new InvalidOperationException("Cannot commit a released reservation");

        CommittedAt = DateTime.UtcNow;
    }

    public void Release()
    {
        if (IsReleased)
            throw new InvalidOperationException("Reservation is already released");

        if (IsCommitted)
            throw new InvalidOperationException("Cannot release a committed reservation");

        ReleasedAt = DateTime.UtcNow;
    }
}