namespace WeatherForecast.Domain.Entities;

public sealed class Location
{
    public required string Name { get; init; }
    public required string Region { get; init; }
    public required string Country { get; init; }
    public required double Latitude { get; init; }
    public required double Longitude { get; init; }
    public required string TimeZone { get; init; }
    public required DateTime LocalTime { get; init; }
}
