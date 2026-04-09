namespace WeatherForecast.Domain.Entities;

/// <summary>
/// Aggregated weather data for the dashboard view.
/// </summary>
public sealed class WeatherDashboard
{
    public required Location Location { get; init; }
    public required CurrentWeather Current { get; init; }
    public required List<HourForecast> HourlyForecast { get; init; }
    public required List<DayForecast> DailyForecast { get; init; }
}
