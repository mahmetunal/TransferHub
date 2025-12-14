using Account.Domain.Events;
using Shared.Common.ValueObjects;
using AccountEntity = Account.Domain.Entities.Account;

namespace Account.Domain.Tests.Entities;

public sealed class AccountTests
{
    [Fact]
    public void Create_WithValidData_ReturnsAccount()
    {
        var accountId = Guid.NewGuid();
        var iban = IBAN.Create("TR330006100519786457841326");
        var initialBalance = Money.Create(1000m, Currency.Create("TRY"));
        var ownerId = Guid.NewGuid().ToString();

        var account = AccountEntity.Create(accountId, iban, initialBalance, ownerId);

        Assert.Equal(accountId, account.Id);
        Assert.Equal(iban, account.Iban);
        Assert.Equal(initialBalance, account.Balance);
        Assert.Equal(ownerId, account.OwnerId);
        Assert.True(account.IsActive);
    }

    [Fact]
    public void Create_WithNullIban_ThrowsArgumentNullException()
    {
        var accountId = Guid.NewGuid();
        var initialBalance = Money.Create(1000m, Currency.Create("TRY"));
        var ownerId = Guid.NewGuid().ToString();

        Assert.Throws<ArgumentNullException>(() =>
            AccountEntity.Create(accountId, null!, initialBalance, ownerId));
    }

    [Fact]
    public void Create_WithNegativeBalance_ThrowsArgumentException()
    {
        var accountId = Guid.NewGuid();
        var iban = IBAN.Create("TR330006100519786457841326");
        var ownerId = Guid.NewGuid().ToString();

        Assert.Throws<ArgumentException>(() =>
        {
            var negativeBalance = Money.Create(-100m, Currency.Create("TRY"));
            AccountEntity.Create(accountId, iban, negativeBalance, ownerId);
        });
    }

    [Fact]
    public void Create_WithNullOwnerId_ThrowsArgumentException()
    {
        var accountId = Guid.NewGuid();
        var iban = IBAN.Create("TR330006100519786457841326");
        var initialBalance = Money.Create(1000m, Currency.Create("TRY"));

        Assert.Throws<ArgumentException>(() =>
            AccountEntity.Create(accountId, iban, initialBalance, null!));
        Assert.Throws<ArgumentException>(() =>
            AccountEntity.Create(accountId, iban, initialBalance, string.Empty));
    }

    [Fact]
    public void ReserveBalance_WithSufficientBalance_DecreasesBalance()
    {
        var account = CreateValidAccount(1000m);
        var amount = Money.Create(500m, Currency.Create("TRY"));
        var reservationId = Guid.NewGuid();
        var initialBalance = account.Balance.Amount;

        account.ReserveBalance(amount, reservationId);

        Assert.Equal(initialBalance - amount.Amount, account.Balance.Amount);
    }

    [Fact]
    public void ReserveBalance_WithInsufficientBalance_ThrowsInvalidOperationException()
    {
        var account = CreateValidAccount(100m);
        var amount = Money.Create(500m, Currency.Create("TRY"));
        var reservationId = Guid.NewGuid();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            account.ReserveBalance(amount, reservationId));
        Assert.Contains("Insufficient balance", exception.Message);
    }

    [Fact]
    public void ReserveBalance_WithDifferentCurrency_ThrowsInvalidOperationException()
    {
        var account = CreateValidAccount(1000m);
        var amount = Money.Create(500m, Currency.Create("USD"));
        var reservationId = Guid.NewGuid();

        Assert.Throws<InvalidOperationException>(() =>
            account.ReserveBalance(amount, reservationId));
    }

    [Fact]
    public void CommitReservation_WithValidReservation_RaisesEvent()
    {
        var account = CreateValidAccount(1000m);
        var amount = Money.Create(500m, Currency.Create("TRY"));
        var reservationId = Guid.NewGuid();
        account.ReserveBalance(amount, reservationId);
        account.DomainEvents.Clear();

        account.CommitReservation(amount, reservationId);

        Assert.Single(account.DomainEvents);
        Assert.IsType<BalanceCommittedEvent>(account.DomainEvents.First());
    }

    [Fact]
    public void ReleaseReservation_WithValidReservation_ReleasesAmount()
    {
        var account = CreateValidAccount(1000m);
        var amount = Money.Create(500m, Currency.Create("TRY"));
        var reservationId = Guid.NewGuid();
        var balanceAfterReservation = account.Balance.Amount - amount.Amount;
        account.ReserveBalance(amount, reservationId);
        account.DomainEvents.Clear();

        account.ReleaseReservation(amount, reservationId);

        Assert.Equal(balanceAfterReservation + amount.Amount, account.Balance.Amount);
    }

    [Fact]
    public void Credit_WithValidAmount_IncreasesBalance()
    {
        var account = CreateValidAccount(1000m);
        var amount = Money.Create(500m, Currency.Create("TRY"));
        var initialBalance = account.Balance.Amount;

        account.Credit(amount);

        Assert.Equal(initialBalance + amount.Amount, account.Balance.Amount);
    }

    [Fact]
    public void Credit_WithDifferentCurrency_ThrowsInvalidOperationException()
    {
        var account = CreateValidAccount(1000m);
        var amount = Money.Create(500m, Currency.Create("USD"));

        Assert.Throws<InvalidOperationException>(() => account.Credit(amount));
    }

    [Fact]
    public void Deactivate_SetsIsActiveToFalse()
    {
        var account = CreateValidAccount(1000m);

        account.Deactivate();

        Assert.False(account.IsActive);
    }

    [Fact]
    public void Activate_SetsIsActiveToTrue()
    {
        var account = CreateValidAccount(1000m);
        account.Deactivate();

        account.Activate();

        Assert.True(account.IsActive);
    }

    private static AccountEntity CreateValidAccount(decimal initialBalanceAmount)
    {
        var accountId = Guid.NewGuid();
        var iban = IBAN.Create("TR330006100519786457841326");
        var initialBalance = Money.Create(initialBalanceAmount, Currency.Create("TRY"));
        var ownerId = Guid.NewGuid().ToString();
        return AccountEntity.Create(accountId, iban, initialBalance, ownerId);
    }
}