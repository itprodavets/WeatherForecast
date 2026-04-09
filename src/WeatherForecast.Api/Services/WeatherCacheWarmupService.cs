using MediatR;
using WeatherForecast.Application.Queries.GetWeatherDashboard;

namespace WeatherForecast.Api.Services;

/// <summary>
/// Background service that pre-warms weather cache on a schedule.
/// Ensures the first user request always hits warm cache.
/// </summary>
public sealed partial class WeatherCacheWarmupService(
    IServiceScopeFactory scopeFactory,
    ILogger<WeatherCacheWarmupService> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(10);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        LogStarted(logger);

        // Initial warmup on startup
        await WarmupCacheAsync(stoppingToken);

        using var timer = new PeriodicTimer(Interval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await WarmupCacheAsync(stoppingToken);
        }
    }

    private async Task WarmupCacheAsync(CancellationToken cancellationToken)
    {
        try
        {
            LogWarmingUp(logger);

            using var scope = scopeFactory.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            await mediator.Send(new GetWeatherDashboardQuery(), cancellationToken);

            LogWarmupComplete(logger);
        }
        catch (Exception ex)
        {
            LogWarmupFailed(logger, ex);
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Weather cache warmup service started")]
    private static partial void LogStarted(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Warming up weather cache")]
    private static partial void LogWarmingUp(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Weather cache warmup completed")]
    private static partial void LogWarmupComplete(ILogger logger);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Weather cache warmup failed")]
    private static partial void LogWarmupFailed(ILogger logger, Exception ex);
}
