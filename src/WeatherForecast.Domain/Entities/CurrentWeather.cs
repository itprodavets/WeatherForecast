using WeatherForecast.Domain.ValueObjects;

namespace WeatherForecast.Domain.Entities;

public sealed class CurrentWeather
{
    public required Temperature Temperature { get; init; }
    public required Wind Wind { get; init; }
    public required int Humidity { get; init; }
    public required double PressureMb { get; init; }
    public required int CloudCover { get; init; }
    public required double UvIndex { get; init; }
    public required double VisibilityKm { get; init; }
    public required string ConditionText { get; init; }
    public required string ConditionIconUrl { get; init; }
    public required bool IsDay { get; init; }
    public required DateTime LastUpdated { get; init; }
}
