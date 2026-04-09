namespace WeatherForecast.Domain.ValueObjects;

public sealed record Coordinates(double Latitude, double Longitude)
{
    public static readonly Coordinates Moscow = new(55.7558, 37.6173);
}
