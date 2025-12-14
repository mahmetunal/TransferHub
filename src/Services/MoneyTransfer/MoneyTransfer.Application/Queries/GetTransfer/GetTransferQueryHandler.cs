using MediatR;
using Microsoft.Extensions.Logging;
using MoneyTransfer.Application.DTOs;
using MoneyTransfer.Application.Repositories;

namespace MoneyTransfer.Application.Queries.GetTransfer;

public sealed class GetTransferQueryHandler : IRequestHandler<GetTransferQuery, TransferDto?>
{
    private readonly ITransferRepository _transferRepository;
    private readonly ILogger<GetTransferQueryHandler> _logger;

    public GetTransferQueryHandler(
        ITransferRepository transferRepository,
        ILogger<GetTransferQueryHandler> logger)
    {
        _transferRepository = transferRepository ?? throw new ArgumentNullException(nameof(transferRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<TransferDto?> Handle(GetTransferQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "User {RequestedBy} retrieving transfer {TransferId}",
            request.RequestedBy,
            request.TransferId);

        var transfer = await _transferRepository.GetByIdAsync(request.TransferId, cancellationToken);

        if (transfer == null)
        {
            _logger.LogWarning("Transfer {TransferId} not found", request.TransferId);
            return null;
        }

        if (transfer.InitiatedBy == request.RequestedBy)
            return new TransferDto
            {
                Id = transfer.Id,
                SourceAccount = transfer.SourceAccount.Value,
                DestinationAccount = transfer.DestinationAccount.Value,
                Amount = transfer.Amount.Amount,
                Currency = transfer.Amount.Currency.Code,
                Status = transfer.Status.ToString(),
                FailureReason = transfer.FailureReason,
                Reference = transfer.Reference,
                InitiatedBy = transfer.InitiatedBy,
                CreatedAt = transfer.RequestedAt,
                CompletedAt = transfer.CompletedAt
            };

        return null;
    }
}