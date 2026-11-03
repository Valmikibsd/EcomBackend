using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting ApiGateway...");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((ctx, services, cfg) => cfg
        .ReadFrom.Configuration(ctx.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Service", "ApiGateway")
        .Enrich.WithProperty("Environment", ctx.HostingEnvironment.EnvironmentName));

    // ── YARP Reverse Proxy ────────────────────────────────────────────────────
    builder.Services.AddReverseProxy()
        .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

    // ── CORS ──────────────────────────────────────────────────────────────────
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
            policy.SetIsOriginAllowed(_ => true)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials());
        options.AddPolicy("AllowAngular", policy =>
            policy.SetIsOriginAllowed(_ => true)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials());
    });

    var app = builder.Build();

    // ── Serilog request logging for the gateway ───────────────────────────────
    app.UseSerilogRequestLogging(opts =>
    {
        opts.MessageTemplate = "GATEWAY {RequestMethod} {RequestPath} → {StatusCode} in {Elapsed:0.0000} ms";
        // Exclude health-check from log noise
        opts.GetLevel = (ctx, elapsed, ex) =>
            ctx.Request.Path.StartsWithSegments("/health")
                ? Serilog.Events.LogEventLevel.Debug
                : ex != null ? Serilog.Events.LogEventLevel.Error
                : Serilog.Events.LogEventLevel.Information;
    });

    app.UseCors("AllowAngular");

    // Health-check endpoint
    app.MapGet("/health", () => Results.Ok(new
    {
        status      = "healthy",
        service     = "ApiGateway",
        environment = app.Environment.EnvironmentName,
        timestamp   = DateTime.UtcNow
    }));

    app.MapReverseProxy();

    app.Run();
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"\n[FATAL ERROR in ApiGateway]: {ex.Message}");
    if (ex.InnerException != null)
    {
        Console.WriteLine($"[Inner Exception]: {ex.InnerException.Message}");
    }
    Console.ResetColor();
    Log.Fatal(ex, "ApiGateway terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
