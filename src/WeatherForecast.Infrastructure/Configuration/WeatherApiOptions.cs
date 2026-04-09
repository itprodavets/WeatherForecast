using System.ComponentModel.DataAnnotations;

namespace WeatherForecast.Infrastructure.Configuration;

public sealed class WeatherApiOptions
{
    public const string SectionName = "WeatherApi";

    [Required, Url]
    public required string BaseUrl { get; set; }

    [Required, MinLength(1)]
    public required string ApiKey { get; set; }

    [Range(1, 120)]
    public int TimeoutSeconds { get; set; } = 10;
}
