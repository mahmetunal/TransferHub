using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging;
using MoneyTransfer.Application.Commands.CreateTransfer;
using MoneyTransfer.Application.Repositories;
using MoneyTransfer.Domain.Entities;
using Moq;
using Shared.Common.Persistence;
using Shared.Common.Sagas.Events;

namespace MoneyTransfer.Application.Tests.Commands;

public sealed class CreateTransferCommandHandlerTests
{
    private readonly Mock<ITransferRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IPublishEndpoint> _publishEndpointMock;
    private readonly CreateTransferCommandHandler _handler;

    public CreateTransferCommandHandlerTests()
    {
        _repositoryMock = new Mock<ITransferRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _publishEndpointMock = new Mock<IPublishEndpoint>();
        var loggerMock = new Mock<ILogger<CreateTransferCommandHandler>>();

        _handler = new CreateTransferCommandHandler(
            _repositoryMock.Object,
            _unitOfWorkMock.Object,
            _publishEndpointMock.Object,
            loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_CreatesTransfer()
    {
        var command = new CreateTransferCommand
        {
            SourceAccount = "TR330006100519786457841326",
            DestinationAccount = "GB82WEST12345698765432",
            Amount = 100.50m,
            Currency = "TRY",
            Reference = "Test transfer",
            InitiatedBy = "alice"
        };

        Transfer? createdTransfer = null;

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Transfer>(), It.IsAny<CancellationToken>()))
            .Callback<Transfer, CancellationToken>((t, ct) => createdTransfer = t)
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.TransferId.Should().NotBeEmpty();
        createdTransfer.Should().NotBeNull();
        createdTransfer!.SourceAccount.Value.Should().Be(command.SourceAccount);
        createdTransfer.DestinationAccount.Value.Should().Be(command.DestinationAccount);
        createdTransfer.Amount.Amount.Should().Be(command.Amount);
        createdTransfer.Amount.Currency.Code.Should().Be(command.Currency);
        createdTransfer.Reference.Should().Be(command.Reference);
        createdTransfer.Status.Should().Be(TransferStatus.Pending);

        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Transfer>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithValidCommand_PublishesTransferInitiatedEvent()
    {
        var command = new CreateTransferCommand
        {
            SourceAccount = "TR330006100519786457841326",
            DestinationAccount = "GB82WEST12345698765432",
            Amount = 100m,
            Currency = "TRY",
            Reference = "Test transfer",
            InitiatedBy = "alice"
        };

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Transfer>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        await _handler.Handle(command, CancellationToken.None);

        _publishEndpointMock.Verify(
            p => p.Publish(
                It.Is<TransferInitiatedEvent>(e =>
                    e.SourceAccount == command.SourceAccount &&
                    e.DestinationAccount == command.DestinationAccount &&
                    e.Amount == command.Amount &&
                    e.Currency == command.Currency),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithSameSourceAndDestination_ThrowsArgumentException()
    {
        var command = new CreateTransferCommand
        {
            SourceAccount = "TR330006100519786457841326",
            DestinationAccount = "TR330006100519786457841326",
            Amount = 100m,
            Currency = "TRY"
        };

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_PropagatesException()
    {
        var command = new CreateTransferCommand
        {
            SourceAccount = "TR330006100519786457841326",
            DestinationAccount = "GB82WEST12345698765432",
            Amount = 100m,
            Currency = "TRY"
        };

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Transfer>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("Database error"));

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }
}