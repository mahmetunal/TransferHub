using Account.Application.Commands.CreateAccount;
using Account.Application.Commands.TopUpAccount;
using Account.Application.DTOs;
using Account.Application.Queries.GetAccount;
using Account.Application.Queries.GetAccountBalance;
using Account.Application.Queries.GetAccounts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Common.Attributes;
using Shared.Common.Identity;

namespace Account.API.Controllers;

/// <summary>
/// Controller for managing accounts.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public sealed class AccountsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AccountsController> _logger;
    private readonly ICurrentUser _currentUser;

    public AccountsController(
        IMediator mediator,
        ILogger<AccountsController> logger,
        ICurrentUser currentUser)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
    }

    /// <summary>
    /// Gets all accounts owned by the current user.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<AccountDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<AccountDto>>> GetMyAccounts(CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated())
        {
            _logger.LogWarning("Unauthorized access attempt: User not authenticated");
            return Unauthorized("User not authenticated");
        }

        var userId = _currentUser.GetUserId();
        _logger.LogInformation("User {UserId} retrieving their accounts", userId);

        var query = new GetUserAccountsQuery(userId);
        var accounts = await _mediator.Send(query, cancellationToken);

        return Ok(accounts);
    }

    /// <summary>
    /// Creates a new account for the current user.
    /// </summary>
    /// <param name="command">The account creation command.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created account result.</returns>
    [HttpPost]
    [Idempotent(ExpirationHours = 24)]
    [ProducesResponseType(typeof(CreateAccountResult), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CreateAccountResult>> CreateAccount(
        [FromBody] CreateAccountCommand command,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated())
        {
            _logger.LogWarning("Unauthorized access attempt: User not authenticated");
            return Unauthorized("User not authenticated");
        }

        var userId = _currentUser.GetUserId();
        _logger.LogInformation(
            "User {UserId} creating new account with initial balance {Balance} {Currency}",
            userId,
            command.InitialBalance,
            command.Currency);

        var secureCommand = new CreateAccountCommand
        {
            InitialBalance = command.InitialBalance,
            Currency = command.Currency,
            OwnerId = userId
        };

        var result = await _mediator.Send(secureCommand, cancellationToken);

        _logger.LogInformation(
            "User {UserId} created account {AccountId} with IBAN {@Iban}",
            userId,
            result.AccountId,
            result.Iban);

        return CreatedAtAction(nameof(GetAccount), new { iban = result.Iban }, result);
    }

    /// <summary>
    /// Gets an account by IBAN.
    /// </summary>
    /// <param name="iban">The account IBAN.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The account if found and owned by user.</returns>
    [HttpGet("{iban}")]
    [ProducesResponseType(typeof(AccountDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AccountDto>> GetAccount(
        [FromRoute] string iban,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated())
            return Unauthorized("User not authenticated");

        var userId = _currentUser.GetUserId();

        _logger.LogInformation("User {UserId} retrieving account with IBAN {@Iban}", userId, iban);

        var query = new GetAccountQuery { Iban = iban };

        var result = await _mediator.Send(query, cancellationToken);

        if (result == null)
            return NotFound();

        if (result.OwnerId != userId)
        {
            _logger.LogWarning(
                "User {UserId} attempted to access account {AccountId} owned by {OwnerId}",
                userId,
                result.Id,
                result.OwnerId);
            return Forbid();
        }

        return Ok(result);
    }

    /// <summary>
    /// Gets account balance by IBAN.
    /// </summary>
    /// <param name="iban">The account IBAN.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The account balance if found and owned by user.</returns>
    [HttpGet("{iban}/balance")]
    [ProducesResponseType(typeof(GetAccountBalanceResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<GetAccountBalanceResult>> GetAccountBalance(
        [FromRoute] string iban,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated())
            return Unauthorized("User not authenticated");

        var userId = _currentUser.GetUserId();

        _logger.LogInformation("User {UserId} retrieving account balance for IBAN {@Iban}", userId, iban);

        var accountQuery = new GetAccountQuery { Iban = iban };
        var account = await _mediator.Send(accountQuery, cancellationToken);

        if (account == null)
            return NotFound();

        if (account.OwnerId != userId)
        {
            _logger.LogWarning(
                "User {UserId} attempted to access balance of account {AccountId} owned by {OwnerId}",
                userId,
                account.Id,
                account.OwnerId);
            return Forbid();
        }

        var query = new GetAccountBalanceQuery { Iban = iban };
        var result = await _mediator.Send(query, cancellationToken);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    /// <summary>
    /// Tops up an account balance.
    /// </summary>
    /// <param name="iban">The account IBAN.</param>
    /// <param name="command">The top-up command.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated account balance.</returns>
    [HttpPost("{iban}/topup")]
    [Idempotent(ExpirationHours = 24)]
    [ProducesResponseType(typeof(TopUpAccountResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TopUpAccountResult>> TopUpAccount(
        [FromRoute] string iban,
        [FromBody] TopUpAccountCommand command,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated())
        {
            _logger.LogWarning("Unauthorized access attempt: User not authenticated");

            return Unauthorized("User not authenticated");
        }

        var userId = _currentUser.GetUserId();

        _logger.LogInformation(
            "User {UserId} topping up account {@Iban} with {Amount} {Currency}",
            userId,
            iban,
            command.Amount,
            command.Currency);

        var secureCommand = new TopUpAccountCommand
        {
            Iban = iban,
            Amount = command.Amount,
            Currency = command.Currency,
            OwnerId = userId
        };

        try
        {
            var result = await _mediator.Send(secureCommand, cancellationToken);

            _logger.LogInformation(
                "User {UserId} successfully topped up account {@Iban}. New balance: {NewBalance} {Currency}",
                userId,
                iban,
                result.NewBalance,
                result.Currency);

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "User {UserId} attempted to top up account {Iban} they don't own", userId, iban);
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid top-up operation for account {Iban}", iban);
            return BadRequest(ex.Message);
        }
    }
}