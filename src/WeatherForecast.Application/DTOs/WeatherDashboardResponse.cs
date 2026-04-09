namespace WeatherForecast.Application.DTOs;

public sealed record WeatherDashboardResponse
{
    public required LocationDto Location { get; init; }
    public required CurrentWeatherDto Current { get; init; }
    public required List<HourForecastDto> HourlyForecast { get; init; }
    public required List<DayForecastDto> DailyForecast { get; init; }
}

public sealed record LocationDto
{
    public required string Name { get; init; }
    public required string Region { get; init; }
    public required string Country { get; init; }
    public required string TimeZone { get; init; }
    public required DateTime LocalTime { get; init; }
}

public sealed record CurrentWeatherDto
{
    public required double TempCelsius { get; init; }
    public required double FeelsLikeCelsius { get; init; }
    public required int Humidity { get; init; }
    public required double PressureMb { get; init; }
    public required double WindSpeedKph { get; init; }
    public required string WindDirection { get; init; }
    public required int CloudCover { get; init; }
    public required double UvIndex { get; init; }
    public required double VisibilityKm { get; init; }
    public required string ConditionText { get; init; }
    public required string ConditionIconUrl { get; init; }
    public required bool IsDay { get; init; }
    public required DateTime LastUpdated { get; init; }
}

public sealed record HourForecastDto
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

public sealed record DayForecastDto
{
    public required string Date { get; init; }
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
}
