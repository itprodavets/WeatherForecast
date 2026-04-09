using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace WeatherForecast.Application.Behaviors;

public sealed partial class PerformanceBehavior<TRequest, TResponse>(
    ILogger<PerformanceBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private const long WarningThresholdMs = 500;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var response = await next(cancellationToken);
        stopwatch.Stop();

        if (stopwatch.ElapsedMilliseconds > WarningThresholdMs)
        {
            LogLongRunning(logger, typeof(TRequest).Name, stopwatch.ElapsedMilliseconds);
        }

        return response;
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Long running request: {RequestName} ({ElapsedMs}ms)")]
    private static partial void LogLongRunning(ILogger logger, string requestName, long elapsedMs);
}
