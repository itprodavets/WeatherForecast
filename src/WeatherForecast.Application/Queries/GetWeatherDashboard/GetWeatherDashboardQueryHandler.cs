using MediatR;
using Microsoft.Extensions.Logging;
using WeatherForecast.Application.Configuration;
using WeatherForecast.Application.DTOs;
using WeatherForecast.Application.Interfaces;
using WeatherForecast.Application.Mapping;

namespace WeatherForecast.Application.Queries.GetWeatherDashboard;

public sealed partial class GetWeatherDashboardQueryHandler(
    IWeatherApiClient weatherApiClient,
    ILogger<GetWeatherDashboardQueryHandler> logger)
    : IRequestHandler<GetWeatherDashboardQuery, WeatherDashboardResponse>
{
    public async Task<WeatherDashboardResponse> Handle(
        GetWeatherDashboardQuery request,
        CancellationToken cancellationToken)
    {
        LogFetchingFromApi(logger);

        var coordinates = WeatherDefaults.DefaultCoordinates;

        // Two separate API calls as per requirements (current.json + forecast.json)
        // Executed in parallel for performance
        var currentTask = weatherApiClient.GetCurrentWeatherAsync(coordinates, cancellationToken);
        var forecastTask = weatherApiClient.GetForecastAsync(coordinates, WeatherDefaults.ForecastDays, cancellationToken);

        var (location, current) = await currentTask;
        var (_, days) = await forecastTask;

        var now = location.LocalTime;
        var today = DateOnly.FromDateTime(now);
        var endOfTomorrow = today.AddDays(2).ToDateTime(TimeOnly.MinValue);

        // Filter: remaining hours today + all hours tomorrow
        var hourlyForecast = days
            .SelectMany(d => d.Hours)
            .Where(h => h.Time >= now && h.Time < endOfTomorrow)
            .ToList();

        return WeatherDashboardMapper.ToResponse(location, current, hourlyForecast, days);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Fetching fresh weather data from API")]
    private static partial void LogFetchingFromApi(ILogger logger);
}
