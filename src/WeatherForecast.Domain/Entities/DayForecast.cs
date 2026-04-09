namespace WeatherForecast.Domain.Entities;

public sealed class DayForecast
{
    public required DateOnly Date { get; init; }
    public required double MaxTempCelsius { get; init; }
    public required double MinTempCelsius { get; init; }
    public required double AvgTempCelsius { get; init; }
    public required double MaxWindSpeedKph { get; init; }
    public required int AvgHumidity { get; init; }
    public required int ChanceOfRain { get; init; }
    public required double TotalPrecipitationMm { get; init; }
    public required double UvIndex { get; init; }
    public required string ConditionText { get; init; }
    public required string ConditionIconUrl { get; init; }
    public required List<HourForecast> Hours { get; init; }
}
