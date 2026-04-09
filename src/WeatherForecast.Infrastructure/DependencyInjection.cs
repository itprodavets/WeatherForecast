using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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
        services.AddWeatherApiClient();
        services.AddCaching(configuration);

        return services;
    }

    private static void AddWeatherApiClient(this IServiceCollection services)
    {
        services.AddOptions<WeatherApiOptions>()
            .BindConfiguration(WeatherApiOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddHttpClient<IWeatherApiClient, WeatherApiClient>((sp, client) =>
        {
            var opts = sp.GetRequiredService<IOptions<WeatherApiOptions>>().Value;
            client.BaseAddress = new Uri(opts.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(opts.TimeoutSeconds);
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
        services.AddOptions<CacheOptions>()
            .BindConfiguration(CacheOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

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
