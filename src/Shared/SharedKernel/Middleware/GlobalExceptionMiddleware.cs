using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace SharedKernel.Middleware;

/// <summary>
/// Global exception handling middleware.
/// Catches any unhandled exception, logs it with full context,
/// and returns a consistent RFC-7807 Problem-Details JSON response.
/// </summary>
public sealed class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var traceId = context.TraceIdentifier;

        // Map known exception types to HTTP status codes
        var (statusCode, title) = exception switch
        {
            ArgumentNullException or ArgumentException => (HttpStatusCode.BadRequest,       "Bad Request"),
            UnauthorizedAccessException                => (HttpStatusCode.Unauthorized,      "Unauthorized"),
            KeyNotFoundException                       => (HttpStatusCode.NotFound,          "Not Found"),
            InvalidOperationException                  => (HttpStatusCode.UnprocessableEntity,"Unprocessable Entity"),
            NotImplementedException                    => (HttpStatusCode.NotImplemented,    "Not Implemented"),
            TimeoutException                           => (HttpStatusCode.GatewayTimeout,    "Gateway Timeout"),
            _                                          => (HttpStatusCode.InternalServerError,"Internal Server Error")
        };

        _logger.LogError(exception,
            "Unhandled exception [{ExceptionType}] on {Method} {Path} | TraceId: {TraceId} | StatusCode: {StatusCode}",
            exception.GetType().Name,
            context.Request.Method,
            context.Request.Path,
            traceId,
            (int)statusCode);

        context.Response.StatusCode  = (int)statusCode;
        context.Response.ContentType = "application/problem+json";

        var problem = new
        {
            type     = $"https://httpstatuses.com/{(int)statusCode}",
            title,
            status   = (int)statusCode,
            detail   = exception.Message,
            traceId,
            // Only include stack trace in non-production environments
            stackTrace = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") != "Production"
                ? exception.StackTrace
                : null
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(problem, _jsonOptions));
    }
}
