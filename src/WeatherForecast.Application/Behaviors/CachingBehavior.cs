using MediatR;
using Microsoft.Extensions.Logging;
using WeatherForecast.Application.Interfaces;

namespace WeatherForecast.Application.Behaviors;

/// <summary>
/// MediatR pipeline behavior that automatically caches responses
/// for requests implementing <see cref="ICacheable"/>.
/// Uses GetOrCreateAsync to prevent cache stampede.
/// </summary>
public sealed partial class CachingBehavior<TRequest, TResponse>(
    ICacheService cacheService,
    ILogger<CachingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICacheable
    where TResponse : class
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var cacheKey = request.CacheKey;

        return await cacheService.GetOrCreateAsync<TResponse>(
            cacheKey,
            async ct =>
            {
                LogCacheMiss(logger, cacheKey);
                var response = await next(ct);
                LogCached(logger, cacheKey, request.CacheDuration.TotalMinutes);
                return response;
            },
            request.CacheDuration,
            cancellationToken);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Cache miss for {CacheKey}, fetching from source")]
    private static partial void LogCacheMiss(ILogger logger, string cacheKey);

    [LoggerMessage(Level = LogLevel.Information, Message = "Cached {CacheKey} for {Duration} minutes")]
    private static partial void LogCached(ILogger logger, string cacheKey, double duration);
}
