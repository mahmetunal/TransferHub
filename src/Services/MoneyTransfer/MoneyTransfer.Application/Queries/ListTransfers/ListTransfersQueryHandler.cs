using MediatR;
using Microsoft.Extensions.Logging;
using MoneyTransfer.Application.DTOs;
using MoneyTransfer.Application.Repositories;
using MoneyTransfer.Domain.Entities;

namespace MoneyTransfer.Application.Queries.ListTransfers;

public sealed class ListTransfersQueryHandler : IRequestHandler<ListTransfersQuery, ListTransfersResult>
{
    private readonly ITransferRepository _transferRepository;
    private readonly ILogger<ListTransfersQueryHandler> _logger;

    public ListTransfersQueryHandler(
        ITransferRepository transferRepository,
        ILogger<ListTransfersQueryHandler> logger)
    {
        _transferRepository = transferRepository ?? throw new ArgumentNullException(nameof(transferRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ListTransfersResult> Handle(ListTransfersQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Listing transfers for user {InitiatedBy}. Page: {PageNumber}, PageSize: {PageSize}, Status: {Status}",
            request.InitiatedBy,
            request.PageNumber,
            request.PageSize,
            request.Status ?? "All");

        IEnumerable<Transfer> allTransfers;

        if (!string.IsNullOrEmpty(request.Status))
        {
            var status = Enum.Parse<TransferStatus>(request.Status, ignoreCase: true);
            allTransfers = await _transferRepository.GetByStatusAsync(status, cancellationToken);
        }
        else
        {
            allTransfers = await _transferRepository.GetAllAsync(cancellationToken);
        }

        var filteredTransfers = allTransfers
            .Where(t => t.InitiatedBy == request.InitiatedBy);

        if (!string.IsNullOrEmpty(request.SourceAccount))
            filteredTransfers = filteredTransfers.Where(t =>
                t.SourceAccount.Value.Contains(request.SourceAccount, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrEmpty(request.DestinationAccount))
            filteredTransfers = filteredTransfers.Where(t =>
                t.DestinationAccount.Value.Contains(request.DestinationAccount, StringComparison.OrdinalIgnoreCase));

        if (request.FromDate.HasValue)
            filteredTransfers = filteredTransfers.Where(t => t.RequestedAt >= request.FromDate.Value);

        if (request.ToDate.HasValue)
            filteredTransfers = filteredTransfers.Where(t => t.RequestedAt <= request.ToDate.Value);

        var transferLists = filteredTransfers.ToList();

        var totalCount = transferLists.Count;

        var transfers = transferLists
            .OrderByDescending(t => t.RequestedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var transferDtos = transfers
            .Select(t => new TransferDto
            {
                Id = t.Id,
                SourceAccount = t.SourceAccount.Value,
                DestinationAccount = t.DestinationAccount.Value,
                Amount = t.Amount.Amount,
                Currency = t.Amount.Currency.Code,
                Status = t.Status.ToString(),
                FailureReason = t.FailureReason,
                Reference = t.Reference,
                InitiatedBy = t.InitiatedBy,
                CreatedAt = t.RequestedAt,
                CompletedAt = t.CompletedAt
            }).ToList();

        _logger.LogInformation(
            "Found {TotalCount} transfers for user {InitiatedBy}, returning page {PageNumber}",
            totalCount,
            request.InitiatedBy,
            request.PageNumber);

        return new ListTransfersResult
        {
            Transfers = transferDtos,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}