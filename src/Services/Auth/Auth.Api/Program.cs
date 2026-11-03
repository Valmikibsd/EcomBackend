using System.Text;
using Auth.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using SharedKernel.Middleware;

// ── Bootstrap Serilog immediately so startup errors are captured ──────────────
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting Auth.Api...");

    var builder = WebApplication.CreateBuilder(args);

    // ── Serilog: replace default logging ─────────────────────────────────────
    builder.Host.UseSerilog((ctx, services, cfg) => cfg
        .ReadFrom.Configuration(ctx.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Service", "Auth.Api")
        .Enrich.WithProperty("Environment", ctx.HostingEnvironment.EnvironmentName));

    // ── Services ──────────────────────────────────────────────────────────────
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();

    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "Ecom Auth Service API", Version = "v1" });
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer"
        });
        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                },
                Array.Empty<string>()
            }
        });
    });

    builder.Services.AddAuthInfrastructure(builder.Configuration);

    var jwtKey      = builder.Configuration["Jwt:Key"]      ?? "Ecom_SuperSecret_Jwt_Security_Key_2026_DotNet9_Dapper_Architecture!";
    var jwtIssuer   = builder.Configuration["Jwt:Issuer"]   ?? "EcomAuthService";
    var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "EcomFrontendApp";

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer           = true,
                ValidateAudience         = true,
                ValidateLifetime         = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer              = jwtIssuer,
                ValidAudience            = jwtAudience,
                IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                ClockSkew                = TimeSpan.Zero
            };
        });

    builder.Services.AddAuthorization();

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
            policy.SetIsOriginAllowed(_ => true)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials());
    });

    // ── Build ─────────────────────────────────────────────────────────────────
    var app = builder.Build();

    // ── Middleware pipeline (order matters) ───────────────────────────────────
    app.UseGlobalExceptionHandler(); // ← must be FIRST
    app.UseRequestLogging();         // ← request/response timing
    app.UseSerilogRequestLogging(opts =>
    {
        opts.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    });

    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Auth Service v1"));

    app.UseCors("AllowAll");
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    app.MapGet("/health", () => Results.Ok(new
    {
        status      = "healthy",
        service     = "Auth.Api",
        environment = app.Environment.EnvironmentName,
        timestamp   = DateTime.UtcNow
    }));

    app.Run();
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"\n[FATAL ERROR in Auth.Api]: {ex.Message}");
    if (ex.InnerException != null)
    {
        Console.WriteLine($"[Inner Exception]: {ex.InnerException.Message}");
    }
    Console.ResetColor();
    Log.Fatal(ex, "Auth.Api terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
