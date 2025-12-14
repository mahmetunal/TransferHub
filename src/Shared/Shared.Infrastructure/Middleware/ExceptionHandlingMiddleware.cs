using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Shared.Common.Logging;

namespace Shared.Infrastructure.Middleware;

/// <summary>
/// Middleware for global exception handling.
/// </summary>
public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var maskedMessage = ex.Message.MaskPii();

            _logger.LogError(
                ex,
                "An unhandled exception occurred. Message: {Message}, CorrelationId: {CorrelationId}",
                maskedMessage,
                context.Items["CorrelationId"]);

            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var code = exception switch
        {
            ArgumentException or InvalidOperationException => HttpStatusCode.BadRequest,
            KeyNotFoundException => HttpStatusCode.NotFound,
            UnauthorizedAccessException => HttpStatusCode.Unauthorized,
            _ => HttpStatusCode.InternalServerError
        };

        var maskedErrorMessage = exception.Message.MaskPii();

        var result = JsonSerializer.Serialize(new
        {
            error = new
            {
                message = maskedErrorMessage,
                type = exception.GetType().Name,
                correlationId = context.Items["CorrelationId"]
            }
        });

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int) code;

        return context.Response.WriteAsync(result);
    }
}