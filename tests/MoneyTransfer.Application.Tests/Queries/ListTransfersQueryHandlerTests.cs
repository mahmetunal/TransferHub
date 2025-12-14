using FluentAssertions;
using Microsoft.Extensions.Logging;
using MoneyTransfer.Application.Queries.ListTransfers;
using MoneyTransfer.Application.Repositories;
using MoneyTransfer.Domain.Entities;
using Moq;
using Shared.Common.ValueObjects;

namespace MoneyTransfer.Application.Tests.Queries;

public sealed class ListTransfersQueryHandlerTests
{
    private readonly Mock<ITransferRepository> _repositoryMock;
    private readonly ListTransfersQueryHandler _handler;

    public ListTransfersQueryHandlerTests()
    {
        _repositoryMock = new Mock<ITransferRepository>();
        var loggerMock = new Mock<ILogger<ListTransfersQueryHandler>>();

        _handler = new ListTransfersQueryHandler(_repositoryMock.Object, loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidQuery_ReturnsPaginatedResults()
    {
        var transfers = new List<Transfer>
        {
            Transfer.Create(
                Guid.NewGuid(),
                IBAN.Create("TR330006100519786457841326"),
                IBAN.Create("GB82WEST12345698765432"),
                Money.Create(100m, Currency.Create("TRY")),
                "alice",
                "Transfer 1"),
            Transfer.Create(
                Guid.NewGuid(),
                IBAN.Create("TR330006100519786457841326"),
                IBAN.Create("GB82WEST12345698765432"),
                Money.Create(200m, Currency.Create("TRY")),
                "alice",
                "Transfer 2")
        };

        _repositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(transfers.AsEnumerable());

        var query = new ListTransfersQuery
        {
            PageNumber = 1,
            PageSize = 10,
            InitiatedBy = "alice"
        };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Transfers.Should().HaveCount(2);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.TotalCount.Should().Be(2);

        _repositoryMock.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithPagination_ReturnsCorrectPage()
    {
        var transfers = Enumerable.Range(1, 25)
            .Select(i => Transfer.Create(
                Guid.NewGuid(),
                IBAN.Create("TR330006100519786457841326"),
                IBAN.Create("GB82WEST12345698765432"),
                Money.Create(100m * i, Currency.Create("TRY")),
                "alice",
                $"Transfer {i}"))
            .ToList();

        _repositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(transfers.AsEnumerable());

        var query = new ListTransfersQuery
        {
            PageNumber = 2,
            PageSize = 10,
            InitiatedBy = "alice"
        };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Transfers.Should().HaveCount(10);
        result.PageNumber.Should().Be(2);
        result.PageSize.Should().Be(10);
        result.TotalCount.Should().Be(25);
    }

    [Fact]
    public async Task Handle_WithEmptyResults_ReturnsEmptyList()
    {
        _repositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Transfer>());

        var query = new ListTransfersQuery
        {
            PageNumber = 1,
            PageSize = 10
        };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Transfers.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }
}