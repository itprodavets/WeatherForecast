using System.Globalization;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WeatherForecast.Application.Interfaces;
using WeatherForecast.Domain.Entities;
using WeatherForecast.Domain.ValueObjects;
using WeatherForecast.Infrastructure.Configuration;
using WeatherForecast.Infrastructure.WeatherApi.Models;

namespace WeatherForecast.Infrastructure.WeatherApi;

internal sealed partial class WeatherApiClient(
    HttpClient httpClient,
    IOptions<WeatherApiOptions> options,
    ILogger<WeatherApiClient> logger) : IWeatherApiClient
{
    private readonly WeatherApiOptions _options = options.Value;

    public async Task<(Location Location, CurrentWeather Current)> GetCurrentWeatherAsync(
        Coordinates coordinates,
        CancellationToken cancellationToken = default)
    {
        var url = BuildUrl("current.json", coordinates);

        LogFetchingCurrent(logger, coordinates.Latitude, coordinates.Longitude);

        var response = await httpClient.GetFromJsonAsync<CurrentResponse>(url, cancellationToken)
            ?? throw new InvalidOperationException("Weather API returned null response for current weather");

        var location = WeatherApiMapper.MapToLocation(response.Location);
        var current = WeatherApiMapper.MapToCurrent(response.Current);

        return (location, current);
    }

    public async Task<(Location Location, List<DayForecast> Days)> GetForecastAsync(
        Coordinates coordinates,
        int days,
        CancellationToken cancellationToken = default)
    {
        var url = BuildUrl("forecast.json", coordinates, $"&days={days}");

        LogFetchingForecast(logger, coordinates.Latitude, coordinates.Longitude, days);

        var response = await httpClient.GetFromJsonAsync<ForecastResponse>(url, cancellationToken)
            ?? throw new InvalidOperationException("Weather API returned null response for forecast");

        var location = WeatherApiMapper.MapToLocation(response.Location);
        var dayForecasts = response.Forecast.ForecastDay.Select(WeatherApiMapper.MapToDay).ToList();

        return (location, dayForecasts);
    }

    private string BuildUrl(string endpoint, Coordinates coordinates, string? extra = null)
    {
        var q = string.Create(CultureInfo.InvariantCulture, $"{coordinates.Latitude},{coordinates.Longitude}");
        return $"/v1/{endpoint}?key={_options.ApiKey}&q={q}{extra}";
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Fetching current weather for ({Lat}, {Lon})")]
    private static partial void LogFetchingCurrent(ILogger logger, double lat, double lon);

    [LoggerMessage(Level = LogLevel.Information, Message = "Fetching {Days}-day forecast for ({Lat}, {Lon})")]
    private static partial void LogFetchingForecast(ILogger logger, double lat, double lon, int days);
}
