using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace WeatherForecast.Application.Behaviors;

public sealed partial class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private const long WarningThresholdMs = 500;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        LogHandling(logger, requestName);

        var stopwatch = Stopwatch.StartNew();
        var response = await next(cancellationToken);
        stopwatch.Stop();

        var elapsedMs = stopwatch.ElapsedMilliseconds;

        if (elapsedMs > WarningThresholdMs)
        {
            LogLongRunning(logger, requestName, elapsedMs);
        }
        else
        {
            LogHandled(logger, requestName, elapsedMs);
        }

        return response;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Handling {RequestName}")]
    private static partial void LogHandling(ILogger logger, string requestName);

    [LoggerMessage(Level = LogLevel.Information, Message = "Handled {RequestName} in {ElapsedMs}ms")]
    private static partial void LogHandled(ILogger logger, string requestName, long elapsedMs);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Long running request: {RequestName} ({ElapsedMs}ms)")]
    private static partial void LogLongRunning(ILogger logger, string requestName, long elapsedMs);
}
