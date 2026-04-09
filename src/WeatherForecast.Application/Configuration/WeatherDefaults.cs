using WeatherForecast.Domain.ValueObjects;

namespace WeatherForecast.Application.Configuration;

/// <summary>
/// Application-level defaults for weather data retrieval.
/// </summary>
public static class WeatherDefaults
{
    /// <summary>
    /// Default coordinates — Moscow, Russia (as per requirements).
    /// </summary>
    public static readonly Coordinates DefaultCoordinates = new(55.7558, 37.6173);

    /// <summary>
    /// Number of forecast days to request from the API.
    /// </summary>
    public const int ForecastDays = 3;
}
