namespace WeatherForecast.Domain.ValueObjects;

public sealed record Wind(double SpeedKph, string Direction, int Degree);
