using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace SharedKernel.Middleware;

/// <summary>
/// HTTP request/response logging middleware.
/// Logs method, path, query, status code, and elapsed time for every request.
/// Skips health-check and swagger endpoints to reduce noise.
/// </summary>
public sealed class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    // Paths to skip (health checks, swagger noise)
    private static readonly string[] _skipPaths =
    [
        "/health", "/swagger", "/favicon.ico"
    ];

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next   = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        // Skip noisy paths
        if (_skipPaths.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
        {
            await _next(context);
            return;
        }

        var sw        = Stopwatch.StartNew();
        var traceId   = context.TraceIdentifier;
        var method    = context.Request.Method;
        var queryStr  = context.Request.QueryString.HasValue ? context.Request.QueryString.Value : string.Empty;
        var clientIp  = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var userAgent = context.Request.Headers.UserAgent.ToString();

        _logger.LogInformation(
            "→ {Method} {Path}{Query} | IP: {ClientIp} | TraceId: {TraceId}",
            method, path, queryStr, clientIp, traceId);

        try
        {
            await _next(context);
        }
        finally
        {
            sw.Stop();
            var statusCode = context.Response.StatusCode;
            var level      = statusCode >= 500 ? LogLevel.Error
                           : statusCode >= 400 ? LogLevel.Warning
                           : LogLevel.Information;

            _logger.Log(level,
                "← {Method} {Path} | {StatusCode} | {ElapsedMs}ms | TraceId: {TraceId}",
                method, path, statusCode, sw.ElapsedMilliseconds, traceId);
        }
    }
}
