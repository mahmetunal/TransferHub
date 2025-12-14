using Account.Application.Queries.GetAccountBalance;
using Account.Application.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Shared.Common.ValueObjects;
using AccountEntity = Account.Domain.Entities.Account;

namespace Account.Application.Tests.Queries;

public sealed class GetAccountBalanceQueryHandlerTests
{
    private readonly Mock<IAccountRepository> _repositoryMock;
    private readonly GetAccountBalanceQueryHandler _handler;

    public GetAccountBalanceQueryHandlerTests()
    {
        _repositoryMock = new Mock<IAccountRepository>();
        var loggerMock = new Mock<ILogger<GetAccountBalanceQueryHandler>>();

        _handler = new GetAccountBalanceQueryHandler(_repositoryMock.Object, loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithExistingAccount_ReturnsBalance()
    {
        var iban = IBAN.Create("TR330006100519786457841326");

        var account = AccountEntity.Create(
            Guid.NewGuid(),
            iban,
            Money.Create(5000m, Currency.Create("TRY")),
            Guid.NewGuid().ToString());

        _repositoryMock
            .Setup(r => r.GetByIbanAsync(iban.Value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        var query = new GetAccountBalanceQuery { Iban = iban.Value };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Balance.Should().Be(5000m);
        result.Currency.Should().Be("TRY");
        result.Iban.Should().Be(iban.Value);

        _repositoryMock.Verify(r => r.GetByIbanAsync(iban.Value, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentAccount_ReturnsNull()
    {
        var iban = IBAN.Create("TR330006100519786457841326");

        _repositoryMock
            .Setup(r => r.GetByIbanAsync(iban.Value, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AccountEntity?) null);

        var query = new GetAccountBalanceQuery { Iban = iban.Value };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeNull();
        _repositoryMock.Verify(r => r.GetByIbanAsync(iban.Value, It.IsAny<CancellationToken>()), Times.Once);
    }
}