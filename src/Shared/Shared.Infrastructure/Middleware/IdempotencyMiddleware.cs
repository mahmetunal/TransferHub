using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Shared.Common.Attributes;
using Shared.Common.Idempotency;

namespace Shared.Infrastructure.Middleware;

/// <summary>
/// Middleware that handles idempotent request processing.
/// Checks for X-Idempotency-Key header and returns cached responses for duplicate requests.
/// </summary>
public sealed class IdempotencyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<IdempotencyMiddleware> _logger;

    private static readonly HashSet<string> IdempotentMethods = new(StringComparer.OrdinalIgnoreCase)
    {
        HttpMethods.Post,
        HttpMethods.Put,
        HttpMethods.Patch,
        HttpMethods.Delete
    };

    public IdempotencyMiddleware(
        RequestDelegate next,
        ILogger<IdempotencyMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InvokeAsync(HttpContext context, IIdempotencyService idempotencyService)
    {
        if (!IdempotentMethods.Contains(context.Request.Method))
        {
            await _next(context);
            return;
        }

        var endpoint = context.GetEndpoint();
        var idempotentAttribute = endpoint?.Metadata.GetMetadata<IdempotentAttribute>();

        if (idempotentAttribute == null)
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue(idempotentAttribute.HeaderName, out var idempotencyKey))
        {
            if (idempotentAttribute.Required)
            {
                _logger.LogWarning(
                    "Missing required idempotency key header '{HeaderName}' for endpoint {Path}",
                    idempotentAttribute.HeaderName,
                    context.Request.Path);

                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(new
                {
                    Error = "IdempotencyKeyRequired",
                    Message = $"The '{idempotentAttribute.HeaderName}' header is required for this operation",
                    idempotentAttribute.HeaderName
                });
                return;
            }

            await _next(context);
            return;
        }

        var key = idempotencyKey.ToString();

        if (string.IsNullOrWhiteSpace(key) || key.Length > 255)
        {
            _logger.LogWarning(
                "Invalid idempotency key provided: {IdempotencyKey}",
                key);

            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new
            {
                Error = "InvalidIdempotencyKey",
                Message = "Idempotency key must be between 1 and 255 characters"
            });
            return;
        }

        try
        {
            var (exists, cachedResponse) = await idempotencyService
                .TryGetAsync<IdempotentResponse>(key, context.RequestAborted);

            if (exists && cachedResponse != null)
            {
                _logger.LogInformation(
                    "Returning cached response for idempotency key {IdempotencyKey} on endpoint {Path}",
                    key,
                    context.Request.Path);

                context.Response.StatusCode = cachedResponse.StatusCode;
                context.Response.ContentType = cachedResponse.ContentType;

                context.Response.Headers["X-Idempotent-Replayed"] = "true";

                if (!string.IsNullOrEmpty(cachedResponse.Body))
                {
                    await context.Response.WriteAsync(cachedResponse.Body, context.RequestAborted);
                }

                return;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error retrieving cached response for idempotency key {IdempotencyKey}",
                key);
        }

        var originalBodyStream = context.Response.Body;

        try
        {
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            await _next(context);

            if (context.Response.StatusCode is >= 200 and < 300)
            {
                responseBody.Seek(0, SeekOrigin.Begin);
                var responseBodyText = await new StreamReader(responseBody).ReadToEndAsync();

                var idempotentResponse = new IdempotentResponse
                {
                    StatusCode = context.Response.StatusCode,
                    ContentType = context.Response.ContentType ?? "application/json",
                    Body = responseBodyText
                };

                var expiration = TimeSpan.FromHours(idempotentAttribute.ExpirationHours);

                try
                {
                    await idempotencyService.SaveAsync(key, idempotentResponse, expiration, context.RequestAborted);

                    _logger.LogInformation(
                        "Cached response for idempotency key {IdempotencyKey} on endpoint {Path} with expiration {Expiration}",
                        key,
                        context.Request.Path,
                        expiration);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Failed to cache response for idempotency key {IdempotencyKey}",
                        key);
                }
            }

            responseBody.Seek(0, SeekOrigin.Begin);

            await responseBody.CopyToAsync(originalBodyStream, context.RequestAborted);
        }
        finally
        {
            context.Response.Body = originalBodyStream;
        }
    }

    private sealed class IdempotentResponse
    {
        public int StatusCode { get; init; }
        public string ContentType { get; init; } = null!;
        public string Body { get; init; } = null!;
    }
}