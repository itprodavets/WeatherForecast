namespace WeatherForecast.Infrastructure.Configuration;

public sealed class WeatherApiOptions
{
    public const string SectionName = "WeatherApi";

    public required string BaseUrl { get; set; }
    public required string ApiKey { get; set; }
    public int TimeoutSeconds { get; set; } = 10;
}
