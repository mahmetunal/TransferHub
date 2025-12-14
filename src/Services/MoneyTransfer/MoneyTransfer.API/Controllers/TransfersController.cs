using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoneyTransfer.Application.Commands.CreateTransfer;
using MoneyTransfer.Application.DTOs;
using MoneyTransfer.Application.Queries.GetTransfer;
using MoneyTransfer.Application.Queries.ListTransfers;
using Shared.Common.Attributes;
using Shared.Common.Identity;

namespace MoneyTransfer.API.Controllers;

/// <summary>
/// Controller for managing money transfers.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public sealed class TransfersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<TransfersController> _logger;
    private readonly ICurrentUser _currentUser;

    public TransfersController(
        IMediator mediator,
        ILogger<TransfersController> logger,
        ICurrentUser currentUser)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
    }

    /// <summary>
    /// Creates a new money transfer.
    /// </summary>
    /// <param name="command">The transfer creation command.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created transfer result.</returns>
    [HttpPost]
    [Idempotent(ExpirationHours = 24)]
    [ProducesResponseType(typeof(CreateTransferResult), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CreateTransferResult>> CreateTransfer(
        [FromBody] CreateTransferCommand command,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated())
        {
            _logger.LogWarning("Unauthorized transfer attempt: User not authenticated");

            return Unauthorized("User not authenticated");
        }

        var userId = _currentUser.GetUserId();

        _logger.LogInformation(
            "User {UserId} initiating transfer from {SourceAccount} to {DestinationAccount}, Amount: {Amount} {Currency}",
            userId,
            command.SourceAccount,
            command.DestinationAccount,
            command.Amount,
            command.Currency);

        var secureCommand = new CreateTransferCommand
        {
            SourceAccount = command.SourceAccount,
            DestinationAccount = command.DestinationAccount,
            Amount = command.Amount,
            Currency = command.Currency,
            Reference = command.Reference,
            InitiatedBy = userId
        };

        var result = await _mediator.Send(secureCommand, cancellationToken);

        _logger.LogInformation(
            "Transfer {TransferId} created successfully with status {Status} for user {UserId}. Saga will validate ownership asynchronously.",
            result.TransferId,
            result.Status,
            userId);

        return CreatedAtAction(nameof(GetTransfer), new { transferId = result.TransferId }, result);
    }

    /// <summary>
    /// Gets a transfer by ID.
    /// </summary>
    /// <param name="transferId">The transfer ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The transfer if found.</returns>
    [HttpGet("{transferId}")]
    [ProducesResponseType(typeof(TransferDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(TransferDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TransferDto>> GetTransfer(
        [FromRoute] Guid transferId,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated())
            return Unauthorized("User not authenticated");

        var userId = _currentUser.GetUserId();
        _logger.LogInformation("User {UserId} retrieving transfer {TransferId}", userId, transferId);

        var query = new GetTransferQuery
        {
            TransferId = transferId,
            RequestedBy = userId
        };

        var result = await _mediator.Send(query, cancellationToken);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    /// <summary>
    /// Lists transfers for the current user.
    /// </summary>
    /// <param name="query">The list transfers query.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of transfers initiated by the user.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ListTransfersResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ListTransfersResult>> ListTransfers(
        [FromQuery] ListTransfersQuery query,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated())
            return Unauthorized("User not authenticated");

        var userId = _currentUser.GetUserId();

        _logger.LogInformation(
            "User {UserId} listing their transfers. Page: {PageNumber}, PageSize: {PageSize}",
            userId,
            query.PageNumber,
            query.PageSize);

        var secureQuery = new ListTransfersQuery
        {
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            Status = query.Status,
            SourceAccount = query.SourceAccount,
            DestinationAccount = query.DestinationAccount,
            FromDate = query.FromDate,
            ToDate = query.ToDate,
            InitiatedBy = userId
        };

        var result = await _mediator.Send(secureQuery, cancellationToken);

        return Ok(result);
    }
}