using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WeatherForecast.Application.Interfaces;
using WeatherForecast.Infrastructure.Caching;
using WeatherForecast.Infrastructure.Configuration;
using WeatherForecast.Infrastructure.WeatherApi;

namespace WeatherForecast.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddWeatherApiClient(configuration);
        services.AddCaching(configuration);

        return services;
    }

    private static void AddWeatherApiClient(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<WeatherApiOptions>(
            configuration.GetSection(WeatherApiOptions.SectionName));

        var weatherOptions = configuration
            .GetSection(WeatherApiOptions.SectionName)
            .Get<WeatherApiOptions>()!;

        services.AddHttpClient<IWeatherApiClient, WeatherApiClient>(client =>
        {
            client.BaseAddress = new Uri(weatherOptions.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(weatherOptions.TimeoutSeconds);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        })
        .AddStandardResilienceHandler(options =>
        {
            options.Retry.MaxRetryAttempts = 3;
            options.Retry.Delay = TimeSpan.FromSeconds(1);
            options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(60);
            options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(10);
            options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(30);
        });
    }

    private static void AddCaching(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<CacheOptions>(
            configuration.GetSection(CacheOptions.SectionName));

        var cacheOptions = configuration
            .GetSection(CacheOptions.SectionName)
            .Get<CacheOptions>() ?? new CacheOptions();

        services.AddMemoryCache();

        if (cacheOptions.UseRedis && !string.IsNullOrEmpty(cacheOptions.RedisConnectionString))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = cacheOptions.RedisConnectionString;
                options.InstanceName = "WeatherForecast:";
            });
        }
        else
        {
            services.AddDistributedMemoryCache();
        }

        services.AddSingleton<ICacheService, CacheService>();
    }
}
