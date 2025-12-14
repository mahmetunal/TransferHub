using Account.Application.Queries.GetAccount;
using Account.Application.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Shared.Common.ValueObjects;
using AccountEntity = Account.Domain.Entities.Account;

namespace Account.Application.Tests.Queries;

public sealed class GetAccountQueryHandlerTests
{
    private readonly Mock<IAccountRepository> _repositoryMock;
    private readonly GetAccountQueryHandler _handler;

    public GetAccountQueryHandlerTests()
    {
        _repositoryMock = new Mock<IAccountRepository>();
        var loggerMock = new Mock<ILogger<GetAccountQueryHandler>>();

        _handler = new GetAccountQueryHandler(_repositoryMock.Object, loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithExistingIban_ReturnsAccountDto()
    {
        var accountId = Guid.NewGuid();

        var iban = IBAN.Create("TR330006100519786457841326");

        var account = AccountEntity.Create(
            accountId,
            iban,
            Money.Create(1000m, Currency.Create("TRY")),
            Guid.NewGuid().ToString());

        _repositoryMock
            .Setup(r => r.GetByIbanAsync(iban.Value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        var query = new GetAccountQuery { Iban = iban.Value };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Id.Should().Be(accountId);
        result.Iban.Should().Be(iban.Value);
        result.Balance.Should().Be(account.Balance.Amount);
        result.Currency.Should().Be(account.Balance.Currency.Code);
        result.IsActive.Should().Be(account.IsActive);

        _repositoryMock.Verify(r => r.GetByIbanAsync(iban.Value, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentIban_ReturnsNull()
    {
        var iban = IBAN.Create("TR330006100519786457841326");

        _repositoryMock
            .Setup(r => r.GetByIbanAsync(iban.Value, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AccountEntity?) null);

        var query = new GetAccountQuery { Iban = iban.Value };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeNull();

        _repositoryMock.Verify(r => r.GetByIbanAsync(iban.Value, It.IsAny<CancellationToken>()), Times.Once);
    }
}