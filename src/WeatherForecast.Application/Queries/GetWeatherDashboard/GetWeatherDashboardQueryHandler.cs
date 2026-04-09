using System.Globalization;
using MediatR;
using Microsoft.Extensions.Logging;
using WeatherForecast.Application.Configuration;
using WeatherForecast.Application.DTOs;
using WeatherForecast.Application.Interfaces;
using WeatherForecast.Application.Mapping;
using WeatherForecast.Domain.ValueObjects;

namespace WeatherForecast.Application.Queries.GetWeatherDashboard;

public sealed partial class GetWeatherDashboardQueryHandler(
    IWeatherApiClient weatherApiClient,
    ICacheService cacheService,
    ILogger<GetWeatherDashboardQueryHandler> logger)
    : IRequestHandler<GetWeatherDashboardQuery, WeatherDashboardResponse>
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    private static string BuildCacheKey(Coordinates coordinates) =>
        string.Create(CultureInfo.InvariantCulture, $"weather:dashboard:{coordinates.Latitude}:{coordinates.Longitude}");

    public async Task<WeatherDashboardResponse> Handle(
        GetWeatherDashboardQuery request,
        CancellationToken cancellationToken)
    {
        var coordinates = WeatherDefaults.DefaultCoordinates;
        var cacheKey = BuildCacheKey(coordinates);

        // GetOrCreateAsync prevents cache stampede — only one request hits the API
        // when multiple concurrent requests encounter a cache miss
        var response = await cacheService.GetOrCreateAsync(
            cacheKey,
            ct => FetchWeatherDataAsync(coordinates, ct),
            CacheDuration,
            cancellationToken);

        return response;
    }

    private async Task<WeatherDashboardResponse> FetchWeatherDataAsync(
        Coordinates coordinates,
        CancellationToken cancellationToken)
    {
        LogFetchingFromApi(logger);

        // Two separate API calls as per requirements (current.json + forecast.json)
        // Executed in parallel for performance
        var currentTask = weatherApiClient.GetCurrentWeatherAsync(coordinates, cancellationToken);
        var forecastTask = weatherApiClient.GetForecastAsync(coordinates, WeatherDefaults.ForecastDays, cancellationToken);

        // Await individually for proper exception unwrapping
        // (Task.WhenAll wraps in AggregateException, individual await preserves original type)
        try
        {
            await Task.WhenAll(currentTask, forecastTask);
        }
        catch
        {
            // Swallow — we re-await below for proper exception type propagation
        }

        var (location, current) = await currentTask;
        var (_, days) = await forecastTask;

        var now = location.LocalTime;
        var today = DateOnly.FromDateTime(now);

        // Filter: remaining hours today + all hours tomorrow (2 days window)
        var endOfTomorrow = today.AddDays(2).ToDateTime(TimeOnly.MinValue);

        var hourlyForecast = days
            .SelectMany(d => d.Hours)
            .Where(h => h.Time >= now && h.Time < endOfTomorrow)
            .ToList();

        var response = WeatherDashboardMapper.ToResponse(location, current, hourlyForecast, days);

        LogCached(logger, CacheDuration.TotalMinutes);

        return response;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Fetching fresh weather data from API")]
    private static partial void LogFetchingFromApi(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Weather dashboard cached for {Duration} minutes")]
    private static partial void LogCached(ILogger logger, double duration);
}
