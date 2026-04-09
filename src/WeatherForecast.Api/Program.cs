using System.Globalization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Serilog;
using WeatherForecast.Api.Middleware;
using WeatherForecast.Api.Services;
using WeatherForecast.Application;
using WeatherForecast.Infrastructure;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Serilog
    builder.Host.UseSerilog((context, config) => config
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console(
            outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}",
            formatProvider: CultureInfo.InvariantCulture)
        .WriteTo.File(
            "logs/weather-.log",
            rollingInterval: RollingInterval.Day,
            formatProvider: CultureInfo.InvariantCulture));

    // Clean Architecture layers
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    // Controllers
    builder.Services.AddControllers();

    // Swagger / OpenAPI
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        var xmlFile = $"{typeof(Program).Assembly.GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
        {
            options.IncludeXmlComments(xmlPath);
        }
    });

    // Exception handling
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();

    // Response compression
    builder.Services.AddResponseCompression(options =>
    {
        options.EnableForHttps = true;
    });

    // Rate limiting
    builder.Services.AddRateLimiter(options =>
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

        options.AddFixedWindowLimiter("fixed", limiter =>
        {
            limiter.PermitLimit = 100;
            limiter.Window = TimeSpan.FromMinutes(1);
            limiter.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            limiter.QueueLimit = 10;
        });

        options.AddSlidingWindowLimiter("sliding", limiter =>
        {
            limiter.PermitLimit = 1000;
            limiter.Window = TimeSpan.FromHours(1);
            limiter.SegmentsPerWindow = 6;
        });
    });

    // Health checks
    builder.Services.AddHealthChecks();

    // CORS for React dev server
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowReactDev", policy =>
        {
            policy
                .WithOrigins(
                    "http://localhost:5173",
                    "http://localhost:3000")
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
    });

    // Background service
    builder.Services.AddHostedService<WeatherCacheWarmupService>();

    var app = builder.Build();

    // Middleware pipeline (order matters — exception handler must wrap everything)
    app.UseResponseCompression();
    app.UseExceptionHandler();
    app.UseMiddleware<ResponseTimeMiddleware>();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Weather Forecast API v1");
        });
    }

    app.UseSerilogRequestLogging(options =>
    {
        // Redact API keys and sensitive query parameters from request logs
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestPath", httpContext.Request.Path);
        };
    });
    app.UseCors("AllowReactDev");
    app.UseRateLimiter();

    app.MapControllers().RequireRateLimiting("fixed");
    app.MapHealthChecks("/health");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// Required for WebApplicationFactory in integration tests
namespace WeatherForecast.Api
{
    public partial class Program;
}
