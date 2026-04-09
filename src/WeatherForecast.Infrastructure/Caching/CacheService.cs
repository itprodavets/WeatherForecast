using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WeatherForecast.Application.Interfaces;
using WeatherForecast.Infrastructure.Configuration;

namespace WeatherForecast.Infrastructure.Caching;

internal sealed partial class CacheService(
    IMemoryCache memoryCache,
    IDistributedCache distributedCache,
    IOptions<CacheOptions> cacheOptions,
    ILogger<CacheService> logger) : ICacheService
{
    private readonly CacheOptions _cacheOptions = cacheOptions.Value;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        // L1: Memory cache (fastest)
        if (memoryCache.TryGetValue(key, out T? cached) && cached is not null)
        {
            LogL1Hit(logger, key);
            return cached;
        }

        // L2: Distributed cache (Redis)
        try
        {
            var bytes = await distributedCache.GetAsync(key, cancellationToken);
            if (bytes is not null)
            {
                var value = JsonSerializer.Deserialize<T>(bytes, JsonOptions);
                if (value is not null)
                {
                    LogL2Hit(logger, key);
                    memoryCache.Set(key, value, TimeSpan.FromMinutes(_cacheOptions.MemoryCacheTtlMinutes));
                    return value;
                }
            }
        }
        catch (Exception ex)
        {
            LogL2Error(logger, key, ex);
        }

        LogCacheMiss(logger, key);
        return null;
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan expiration, CancellationToken cancellationToken = default) where T : class
    {
        // L1: Memory cache
        memoryCache.Set(key, value, expiration);

        // L2: Distributed cache
        try
        {
            var bytes = JsonSerializer.SerializeToUtf8Bytes(value, JsonOptions);
            var distributedTtl = TimeSpan.FromMinutes(_cacheOptions.DistributedCacheTtlMinutes);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = distributedTtl > expiration ? distributedTtl : expiration
            };
            await distributedCache.SetAsync(key, bytes, options, cancellationToken);
        }
        catch (Exception ex)
        {
            LogL2WriteError(logger, key, ex);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        memoryCache.Remove(key);

        try
        {
            await distributedCache.RemoveAsync(key, cancellationToken);
        }
        catch (Exception ex)
        {
            LogL2RemoveError(logger, key, ex);
        }
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "Cache L1 hit for key: {Key}")]
    private static partial void LogL1Hit(ILogger logger, string key);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Cache L2 hit for key: {Key}")]
    private static partial void LogL2Hit(ILogger logger, string key);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Cache miss for key: {Key}")]
    private static partial void LogCacheMiss(ILogger logger, string key);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Redis read error for key: {Key}")]
    private static partial void LogL2Error(ILogger logger, string key, Exception ex);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Redis write error for key: {Key}")]
    private static partial void LogL2WriteError(ILogger logger, string key, Exception ex);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Redis remove error for key: {Key}")]
    private static partial void LogL2RemoveError(ILogger logger, string key, Exception ex);
}
