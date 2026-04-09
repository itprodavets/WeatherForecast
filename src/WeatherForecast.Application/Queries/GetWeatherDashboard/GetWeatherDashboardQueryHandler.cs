using System.Globalization;
using MediatR;
using Microsoft.Extensions.Logging;
using WeatherForecast.Application.DTOs;
using WeatherForecast.Application.Interfaces;
using WeatherForecast.Domain.ValueObjects;

namespace WeatherForecast.Application.Queries.GetWeatherDashboard;

public sealed partial class GetWeatherDashboardQueryHandler(
    IWeatherApiClient weatherApiClient,
    ICacheService cacheService,
    ILogger<GetWeatherDashboardQueryHandler> logger)
    : IRequestHandler<GetWeatherDashboardQuery, WeatherDashboardResponse>
{
    private const string CacheKey = "weather:dashboard:moscow";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    public async Task<WeatherDashboardResponse> Handle(
        GetWeatherDashboardQuery request,
        CancellationToken cancellationToken)
    {
        var cached = await cacheService.GetAsync<WeatherDashboardResponse>(CacheKey, cancellationToken);
        if (cached is not null)
        {
            LogCacheHit(logger);
            return cached;
        }

        LogFetchingFromApi(logger);

        var coordinates = Coordinates.Moscow;

        var currentTask = weatherApiClient.GetCurrentWeatherAsync(coordinates, cancellationToken);
        var forecastTask = weatherApiClient.GetForecastAsync(coordinates, days: 3, cancellationToken);

        await Task.WhenAll(currentTask, forecastTask);

        var current = await currentTask;
        var (location, days) = await forecastTask;

        var now = location.LocalTime;
        var tomorrow = DateOnly.FromDateTime(now).AddDays(1);

        var hourlyForecast = days
            .SelectMany(d => d.Hours)
            .Where(h => h.Time >= now && h.Time < tomorrow.AddDays(1).ToDateTime(TimeOnly.MinValue))
            .ToList();

        var response = new WeatherDashboardResponse
        {
            Location = new LocationDto
            {
                Name = location.Name,
                Region = location.Region,
                Country = location.Country,
                TimeZone = location.TimeZone,
                LocalTime = location.LocalTime
            },
            Current = new CurrentWeatherDto
            {
                TempCelsius = current.Temperature.Celsius,
                FeelsLikeCelsius = current.Temperature.FeelsLike,
                Humidity = current.Humidity,
                PressureMb = current.PressureMb,
                WindSpeedKph = current.Wind.SpeedKph,
                WindDirection = current.Wind.Direction,
                CloudCover = current.CloudCover,
                UvIndex = current.UvIndex,
                VisibilityKm = current.VisibilityKm,
                ConditionText = current.ConditionText,
                ConditionIconUrl = current.ConditionIconUrl,
                IsDay = current.IsDay,
                LastUpdated = current.LastUpdated
            },
            HourlyForecast = hourlyForecast.Select(h => new HourForecastDto
            {
                Time = h.Time,
                TempCelsius = h.TempCelsius,
                FeelsLikeCelsius = h.FeelsLikeCelsius,
                ConditionText = h.ConditionText,
                ConditionIconUrl = h.ConditionIconUrl,
                WindSpeedKph = h.WindSpeedKph,
                Humidity = h.Humidity,
                ChanceOfRain = h.ChanceOfRain,
                IsDay = h.IsDay
            }).ToList(),
            DailyForecast = days.Select(d => new DayForecastDto
            {
                Date = d.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                MaxTempCelsius = d.MaxTempCelsius,
                MinTempCelsius = d.MinTempCelsius,
                AvgTempCelsius = d.AvgTempCelsius,
                MaxWindSpeedKph = d.MaxWindSpeedKph,
                AvgHumidity = d.AvgHumidity,
                ChanceOfRain = d.ChanceOfRain,
                TotalPrecipitationMm = d.TotalPrecipitationMm,
                UvIndex = d.UvIndex,
                ConditionText = d.ConditionText,
                ConditionIconUrl = d.ConditionIconUrl
            }).ToList()
        };

        await cacheService.SetAsync(CacheKey, response, CacheDuration, cancellationToken);

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
