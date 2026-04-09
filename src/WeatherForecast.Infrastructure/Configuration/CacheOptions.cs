namespace WeatherForecast.Infrastructure.Configuration;

public sealed class CacheOptions
{
    public const string SectionName = "Cache";

    public bool UseRedis { get; set; }
    public string? RedisConnectionString { get; set; }
    public int MemoryCacheTtlMinutes { get; set; } = 5;
    public int DistributedCacheTtlMinutes { get; set; } = 15;
}
