namespace WeatherForecast.Application.Interfaces;

/// <summary>
/// Marker interface for MediatR requests that should be cached.
/// Implement this on queries to enable automatic caching via CachingBehavior.
/// </summary>
public interface ICacheable
{
    /// <summary>
    /// Unique cache key for this request.
    /// </summary>
    string CacheKey { get; }

    /// <summary>
    /// How long the response should be cached.
    /// </summary>
    TimeSpan CacheDuration { get; }
}
