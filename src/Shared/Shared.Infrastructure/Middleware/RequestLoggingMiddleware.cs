using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Shared.Common.Logging;

namespace Shared.Infrastructure.Middleware;

/// <summary>
/// Middleware for logging HTTP requests and responses.
/// </summary>
public sealed class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var correlationId = context.Items["CorrelationId"]?.ToString() ?? "unknown";

        var path = context.Request.Path.ToString().MaskPii();
        var queryString = context.Request.QueryString.ToString().MaskPii();

        _logger.LogInformation(
            "Incoming request. Method: {Method}, Path: {Path}, QueryString: {QueryString}, CorrelationId: {CorrelationId}",
            context.Request.Method,
            path,
            queryString,
            correlationId);

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();

            var completedPath = context.Request.Path.ToString().MaskPii();

            _logger.LogInformation(
                "Request completed. Method: {Method}, Path: {Path}, StatusCode: {StatusCode}, Duration: {Duration}ms, CorrelationId: {CorrelationId}",
                context.Request.Method,
                completedPath,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds,
                correlationId);
        }
    }
}