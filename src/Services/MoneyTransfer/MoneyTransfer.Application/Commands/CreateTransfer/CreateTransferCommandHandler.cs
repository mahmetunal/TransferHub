using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using MoneyTransfer.Application.Repositories;
using MoneyTransfer.Domain.Entities;
using Shared.Common.Persistence;
using Shared.Common.Sagas.Events;
using Shared.Common.ValueObjects;

namespace MoneyTransfer.Application.Commands.CreateTransfer;

public sealed class CreateTransferCommandHandler : IRequestHandler<CreateTransferCommand, CreateTransferResult>
{
    private readonly ITransferRepository _transferRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<CreateTransferCommandHandler> _logger;

    public CreateTransferCommandHandler(
        ITransferRepository transferRepository,
        IUnitOfWork unitOfWork,
        IPublishEndpoint publishEndpoint,
        ILogger<CreateTransferCommandHandler> logger)
    {
        _transferRepository = transferRepository ?? throw new ArgumentNullException(nameof(transferRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<CreateTransferResult> Handle(CreateTransferCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Creating transfer from {SourceAccount} to {DestinationAccount} for amount {Amount} {Currency}",
            request.SourceAccount,
            request.DestinationAccount,
            request.Amount,
            request.Currency);

        try
        {
            var sourceIban = IBAN.Create(request.SourceAccount);
            var destinationIban = IBAN.Create(request.DestinationAccount);
            var currency = Currency.Create(request.Currency);
            var amount = Money.Create(request.Amount, currency);

            var transfer = Transfer.Create(
                Guid.NewGuid(),
                sourceIban,
                destinationIban,
                amount,
                request.InitiatedBy!,
                request.Reference);

            await _transferRepository.AddAsync(transfer, cancellationToken);

            var transferInitiatedEvent = new TransferInitiatedEvent
            {
                TransferId = transfer.Id,
                SourceAccount = transfer.SourceAccount.Value,
                DestinationAccount = transfer.DestinationAccount.Value,
                Amount = transfer.Amount.Amount,
                Currency = transfer.Amount.Currency.Code,
                InitiatedBy = transfer.InitiatedBy
            };

            await _publishEndpoint.Publish(transferInitiatedEvent, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Transfer {TransferId} created successfully with status {Status}. Saga initiated.",
                transfer.Id,
                transfer.Status);

            return new CreateTransferResult
            {
                TransferId = transfer.Id,
                Status = transfer.Status.ToString(),
                CreatedAt = transfer.RequestedAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create transfer from {SourceAccount} to {DestinationAccount}",
                request.SourceAccount,
                request.DestinationAccount);

            throw;
        }
    }
}