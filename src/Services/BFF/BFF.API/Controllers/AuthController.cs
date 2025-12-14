using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Common.Authentication.Models;
using Shared.Infrastructure.Authentication;

namespace BFF.API.Controllers;

/// <summary>
/// Authentication controller for JWT token generation.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public sealed class AuthController : ControllerBase
{
    private readonly JwtService _jwtService;
    private readonly ILogger<AuthController> _logger;

    private static readonly Dictionary<string, (string UserId, string Email, string Password)> MockUsers = new()
    {
        { "alice", ("user-alice-001", "alice@transferhub.com", "password123") },
        { "bob", ("user-bob-002", "bob@transferhub.com", "password123") },
        { "charlie", ("user-charlie-003", "charlie@transferhub.com", "password123") }
    };

    public AuthController(
        JwtService jwtService,
        ILogger<AuthController> logger)
    {
        _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Generates a JWT token for authentication.
    /// </summary>
    /// <param name="request">Login request with user credentials.</param>
    /// <returns>JWT token.</returns>
    /// <remarks>
    /// Demo users:
    /// - Username: alice, Password: password123
    /// - Username: bob, Password: password123
    /// - Username: charlie, Password: password123
    /// </remarks>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            return Unauthorized(new { Message = "Invalid credentials" });

        var username = request.Username.ToLowerInvariant();

        if (!MockUsers.TryGetValue(username, out var user))
        {
            _logger.LogWarning("Login attempt with unknown username: {Username}", request.Username);
            return Unauthorized(new { Message = "Invalid credentials" });
        }

        if (user.Password != request.Password)
        {
            _logger.LogWarning("Login attempt with incorrect password for user: {Username}", request.Username);
            return Unauthorized(new { Message = "Invalid credentials" });
        }

        var token = _jwtService.GenerateToken(user.UserId, user.Email, new[] { "User" });

        _logger.LogInformation(
            "User {Username} (ID: {UserId}) logged in successfully",
            request.Username,
            user.UserId);

        var loginResponse = new LoginResponse
        {
            Token = token,
            UserId = user.UserId,
            Email = user.Email,
            ExpiresIn = 3600,
            TokenType = "Bearer"
        };

        return Ok(loginResponse);
    }
}