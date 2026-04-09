namespace WeatherForecast.Domain.ValueObjects;

public sealed record TemperatureRange(double MaxCelsius, double MinCelsius, double AvgCelsius);
