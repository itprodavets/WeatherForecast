namespace WeatherForecast.Domain.Entities;

public sealed class HourForecast
{
    public required DateTime Time { get; init; }
    public required double TempCelsius { get; init; }
    public required double FeelsLikeCelsius { get; init; }
    public required string ConditionText { get; init; }
    public required string ConditionIconUrl { get; init; }
    public required double WindSpeedKph { get; init; }
    public required int Humidity { get; init; }
    public required int ChanceOfRain { get; init; }
    public required bool IsDay { get; init; }
}
