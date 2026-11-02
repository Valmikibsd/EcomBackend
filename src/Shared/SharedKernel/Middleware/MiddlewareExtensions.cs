using Microsoft.AspNetCore.Builder;

namespace SharedKernel.Middleware;

/// <summary>
/// Extension methods to register SharedKernel middleware in one line per service.
/// Usage in Program.cs:
///   app.UseGlobalExceptionHandler();
///   app.UseRequestLogging();
/// </summary>
public static class MiddlewareExtensions
{
    /// <summary>Registers the global exception handling middleware (must be first in pipeline).</summary>
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
        => app.UseMiddleware<GlobalExceptionMiddleware>();

    /// <summary>Registers the HTTP request/response logging middleware.</summary>
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app)
        => app.UseMiddleware<RequestLoggingMiddleware>();
}
