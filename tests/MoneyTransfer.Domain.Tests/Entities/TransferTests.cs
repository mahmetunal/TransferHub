using MoneyTransfer.Domain.Entities;
using MoneyTransfer.Domain.Events;
using Shared.Common.ValueObjects;

namespace MoneyTransfer.Domain.Tests.Entities;

public sealed class TransferTests
{
    [Fact]
    public void Create_WithValidData_ReturnsTransfer()
    {
        var transferId = Guid.NewGuid();

        var sourceIban = IBAN.Create("TR330006100519786457841326");
        var destinationIban = IBAN.Create("GB82WEST12345698765432");
        var amount = Money.Create(100m, Currency.Create("TRY"));
        const string reference = "Test transfer";
        const string initiatedBy = "alice";

        var transfer = Transfer.Create(transferId, sourceIban, destinationIban, amount, initiatedBy, reference);

        Assert.Equal(transferId, transfer.Id);
        Assert.Equal(sourceIban, transfer.SourceAccount);
        Assert.Equal(destinationIban, transfer.DestinationAccount);
        Assert.Equal(amount, transfer.Amount);
        Assert.Equal(reference, transfer.Reference);
        Assert.Equal(initiatedBy, transfer.InitiatedBy);
        Assert.Equal(TransferStatus.Pending, transfer.Status);
        Assert.NotEqual(default, transfer.RequestedAt);
    }

    [Fact]
    public void Create_WithNullSourceIban_ThrowsArgumentNullException()
    {
        var transferId = Guid.NewGuid();
        var destinationIban = IBAN.Create("GB82WEST12345698765432");
        var amount = Money.Create(100m, Currency.Create("TRY"));

        Assert.Throws<ArgumentNullException>(() =>
            Transfer.Create(transferId, null!, destinationIban, amount, "Test"));
    }

    [Fact]
    public void Create_WithNullDestinationIban_ThrowsArgumentNullException()
    {
        var transferId = Guid.NewGuid();
        var sourceIban = IBAN.Create("TR330006100519786457841326");
        var amount = Money.Create(100m, Currency.Create("TRY"));

        Assert.Throws<ArgumentNullException>(() =>
            Transfer.Create(transferId, sourceIban, null!, amount, "Test"));
    }

    [Fact]
    public void Create_WithNullAmount_ThrowsArgumentNullException()
    {
        var transferId = Guid.NewGuid();
        var sourceIban = IBAN.Create("TR330006100519786457841326");
        var destinationIban = IBAN.Create("GB82WEST12345698765432");

        Assert.Throws<ArgumentNullException>(() =>
            Transfer.Create(transferId, sourceIban, destinationIban, null!, "Test"));
    }

    [Fact]
    public void Create_RaisesTransferCreatedEvent()
    {
        var transferId = Guid.NewGuid();
        var sourceIban = IBAN.Create("TR330006100519786457841326");
        var destinationIban = IBAN.Create("GB82WEST12345698765432");
        var amount = Money.Create(100m, Currency.Create("TRY"));

        var transfer = Transfer.Create(transferId, sourceIban, destinationIban, amount, "Test");

        Assert.Single(transfer.DomainEvents);
        Assert.IsType<TransferCreatedEvent>(transfer.DomainEvents.First());
    }

    [Fact]
    public void MarkAsProcessing_WhenPending_ChangesStatusToProcessing()
    {
        var transfer = CreateValidTransfer();

        transfer.MarkAsProcessing();

        Assert.Equal(TransferStatus.Processing, transfer.Status);
    }

    [Fact]
    public void MarkAsProcessing_RaisesTransferProcessingEvent()
    {
        var transfer = CreateValidTransfer();
        transfer.DomainEvents.Clear();

        transfer.MarkAsProcessing();

        Assert.Single(transfer.DomainEvents);
        Assert.IsType<TransferProcessingEvent>(transfer.DomainEvents.First());
    }

    [Fact]
    public void MarkAsCompleted_WhenProcessing_ChangesStatusToCompleted()
    {
        var transfer = CreateValidTransfer();
        transfer.MarkAsProcessing();
        transfer.DomainEvents.Clear();

        transfer.MarkAsCompleted();

        Assert.Equal(TransferStatus.Completed, transfer.Status);
        Assert.NotNull(transfer.CompletedAt);
    }

    [Fact]
    public void MarkAsCompleted_RaisesTransferCompletedEvent()
    {
        var transfer = CreateValidTransfer();
        transfer.MarkAsProcessing();
        transfer.DomainEvents.Clear();

        transfer.MarkAsCompleted();

        Assert.Single(transfer.DomainEvents);
        Assert.IsType<TransferCompletedEvent>(transfer.DomainEvents.First());
    }

    [Fact]
    public void MarkAsFailed_WhenProcessing_ChangesStatusToFailed()
    {
        var transfer = CreateValidTransfer();
        transfer.MarkAsProcessing();
        const string reason = "Insufficient balance";
        transfer.DomainEvents.Clear();

        transfer.MarkAsFailed(reason);

        Assert.Equal(TransferStatus.Failed, transfer.Status);
        Assert.Equal(reason, transfer.FailureReason);
    }

    [Fact]
    public void MarkAsFailed_RaisesTransferFailedEvent()
    {
        var transfer = CreateValidTransfer();
        transfer.MarkAsProcessing();
        transfer.DomainEvents.Clear();

        transfer.MarkAsFailed("Test failure");

        Assert.Single(transfer.DomainEvents);
        var failedEvent = Assert.IsType<TransferFailedEvent>(transfer.DomainEvents.First());
        Assert.Equal("Test failure", failedEvent.Reason);
    }

    [Fact]
    public void Cancel_WhenPending_ChangesStatusToCancelled()
    {
        var transfer = CreateValidTransfer();
        const string reason = "User cancelled";
        transfer.DomainEvents.Clear();

        transfer.Cancel(reason);

        Assert.Equal(TransferStatus.Cancelled, transfer.Status);
        Assert.Equal(reason, transfer.FailureReason);
    }

    [Fact]
    public void Cancel_RaisesTransferCancelledEvent()
    {
        var transfer = CreateValidTransfer();
        transfer.DomainEvents.Clear();

        transfer.Cancel("Test cancellation");

        Assert.Single(transfer.DomainEvents);
        var cancelledEvent = Assert.IsType<TransferCancelledEvent>(transfer.DomainEvents.First());
        Assert.Equal("Test cancellation", cancelledEvent.Reason);
    }

    private static Transfer CreateValidTransfer()
    {
        var transferId = Guid.NewGuid();
        var sourceIban = IBAN.Create("TR330006100519786457841326");
        var destinationIban = IBAN.Create("GB82WEST12345698765432");
        var amount = Money.Create(100m, Currency.Create("TRY"));
        return Transfer.Create(transferId, sourceIban, destinationIban, amount, "Test transfer");
    }
}