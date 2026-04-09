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

        var cached = await cacheService.GetAsync<WeatherDashboardResponse>(cacheKey, cancellationToken);
        if (cached is not null)
        {
            LogCacheHit(logger);
            return cached;
        }

        LogFetchingFromApi(logger);

        // Two separate API calls as per requirements (current.json + forecast.json)
        // Executed in parallel for performance
        var currentTask = weatherApiClient.GetCurrentWeatherAsync(coordinates, cancellationToken);
        var forecastTask = weatherApiClient.GetForecastAsync(coordinates, WeatherDefaults.ForecastDays, cancellationToken);

        await Task.WhenAll(currentTask, forecastTask);

        var (location, current) = currentTask.Result;
        var (_, days) = forecastTask.Result;

        var now = location.LocalTime;
        var tomorrow = DateOnly.FromDateTime(now).AddDays(1);

        var hourlyForecast = days
            .SelectMany(d => d.Hours)
            .Where(h => h.Time >= now && h.Time < tomorrow.AddDays(1).ToDateTime(TimeOnly.MinValue))
            .ToList();

        var response = WeatherDashboardMapper.ToResponse(location, current, hourlyForecast, days);

        await cacheService.SetAsync(cacheKey, response, CacheDuration, cancellationToken);

        LogCached(logger, CacheDuration.TotalMinutes);

        return response;
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "Weather dashboard served from cache")]
    private static partial void LogCacheHit(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Fetching fresh weather data from API")]
    private static partial void LogFetchingFromApi(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Weather dashboard cached for {Duration} minutes")]
    private static partial void LogCached(ILogger logger, double duration);
}
