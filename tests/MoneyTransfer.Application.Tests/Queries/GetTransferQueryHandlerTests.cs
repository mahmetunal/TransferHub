using FluentAssertions;
using Microsoft.Extensions.Logging;
using MoneyTransfer.Application.Queries.GetTransfer;
using MoneyTransfer.Application.Repositories;
using MoneyTransfer.Domain.Entities;
using Moq;

namespace MoneyTransfer.Application.Tests.Queries;

public sealed class GetTransferQueryHandlerTests
{
    private readonly Mock<ITransferRepository> _repositoryMock;
    private readonly GetTransferQueryHandler _handler;

    public GetTransferQueryHandlerTests()
    {
        _repositoryMock = new Mock<ITransferRepository>();
        var loggerMock = new Mock<ILogger<GetTransferQueryHandler>>();

        _handler = new GetTransferQueryHandler(_repositoryMock.Object, loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithNonExistentTransferId_ReturnsNull()
    {
        var transferId = Guid.NewGuid();

        _repositoryMock
            .Setup(r => r.GetByIdAsync(transferId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Transfer?) null);

        var query = new GetTransferQuery { TransferId = transferId };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeNull();

        _repositoryMock.Verify(r => r.GetByIdAsync(transferId, It.IsAny<CancellationToken>()), Times.Once);
    }
}