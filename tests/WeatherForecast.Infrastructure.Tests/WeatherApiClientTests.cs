using System.Net;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using WeatherForecast.Domain.ValueObjects;
using WeatherForecast.Infrastructure.Configuration;
using WeatherForecast.Infrastructure.WeatherApi;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace WeatherForecast.Infrastructure.Tests;

public class WeatherApiClientTests : IDisposable
{
    private readonly WireMockServer _server;
    private readonly WeatherApiClient _client;

    public WeatherApiClientTests()
    {
        _server = WireMockServer.Start();

        var options = Options.Create(new WeatherApiOptions
        {
            BaseUrl = _server.Url!,
            ApiKey = "test-key",
            TimeoutSeconds = 10
        });

        var httpClient = new HttpClient { BaseAddress = new Uri(_server.Url!) };

        _client = new WeatherApiClient(httpClient, options, NullLogger<WeatherApiClient>.Instance);
    }

    [Fact]
    public async Task GetCurrentWeatherAsync_ReturnsCurrentWeather()
    {
        // Arrange
        _server.Given(Request.Create().WithPath("/v1/current.json").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody(CurrentWeatherJson));

        // Act
        var result = await _client.GetCurrentWeatherAsync(Coordinates.Moscow);

        // Assert
        result.Should().NotBeNull();
        result.Temperature.Celsius.Should().Be(22.0);
        result.Temperature.FeelsLike.Should().Be(21.0);
        result.Humidity.Should().Be(55);
        result.Wind.SpeedKph.Should().Be(15.1);
        result.Wind.Direction.Should().Be("NW");
        result.ConditionText.Should().Be("Partly cloudy");
        result.IsDay.Should().BeTrue();
    }

    [Fact]
    public async Task GetForecastAsync_ReturnsForecastWithDays()
    {
        // Arrange
        _server.Given(Request.Create().WithPath("/v1/forecast.json").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody(ForecastJson));

        // Act
        var (location, days) = await _client.GetForecastAsync(Coordinates.Moscow, 3);

        // Assert
        location.Should().NotBeNull();
        location.Name.Should().Be("Moscow");
        location.TimeZone.Should().Be("Europe/Moscow");

        days.Should().HaveCount(1);
        days[0].MaxTempCelsius.Should().Be(25.0);
        days[0].MinTempCelsius.Should().Be(14.0);
        days[0].Hours.Should().HaveCount(1);
        days[0].Hours[0].TempCelsius.Should().Be(20.0);
    }

    [Fact]
    public async Task GetCurrentWeatherAsync_WhenApiReturnsError_ThrowsHttpRequestException()
    {
        // Arrange
        _server.Given(Request.Create().WithPath("/v1/current.json").UsingGet())
            .RespondWith(Response.Create().WithStatusCode(HttpStatusCode.InternalServerError));

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(
            () => _client.GetCurrentWeatherAsync(Coordinates.Moscow));
    }

    [Fact]
    public async Task GetCurrentWeatherAsync_CorrectlyMapsIconUrl()
    {
        // Arrange
        _server.Given(Request.Create().WithPath("/v1/current.json").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody(CurrentWeatherJson));

        // Act
        var result = await _client.GetCurrentWeatherAsync(Coordinates.Moscow);

        // Assert — icon URL should be normalized with https:
        result.ConditionIconUrl.Should().StartWith("https://");
    }

    public void Dispose()
    {
        _server.Dispose();
        GC.SuppressFinalize(this);
    }

    private const string CurrentWeatherJson = """
        {
            "location": {
                "name": "Moscow",
                "region": "Moscow City",
                "country": "Russia",
                "lat": 55.75,
                "lon": 37.62,
                "tz_id": "Europe/Moscow",
                "localtime": "2026-04-09 14:00"
            },
            "current": {
                "temp_c": 22.0,
                "feelslike_c": 21.0,
                "humidity": 55,
                "pressure_mb": 1015.0,
                "wind_kph": 15.1,
                "wind_dir": "NW",
                "wind_degree": 315,
                "cloud": 40,
                "uv": 4.0,
                "vis_km": 10.0,
                "condition": {
                    "text": "Partly cloudy",
                    "icon": "//cdn.weatherapi.com/weather/64x64/day/116.png"
                },
                "is_day": 1,
                "last_updated": "2026-04-09 14:00"
            }
        }
        """;

    private const string ForecastJson = """
        {
            "location": {
                "name": "Moscow",
                "region": "Moscow City",
                "country": "Russia",
                "lat": 55.75,
                "lon": 37.62,
                "tz_id": "Europe/Moscow",
                "localtime": "2026-04-09 14:00"
            },
            "forecast": {
                "forecastday": [
                    {
                        "date": "2026-04-09",
                        "day": {
                            "maxtemp_c": 25.0,
                            "mintemp_c": 14.0,
                            "avgtemp_c": 19.5,
                            "maxwind_kph": 20.0,
                            "avghumidity": 60,
                            "daily_chance_of_rain": 30,
                            "totalprecip_mm": 2.5,
                            "uv": 5.0,
                            "condition": {
                                "text": "Partly cloudy",
                                "icon": "//cdn.weatherapi.com/weather/64x64/day/116.png"
                            }
                        },
                        "hour": [
                            {
                                "time": "2026-04-09 15:00",
                                "temp_c": 20.0,
                                "feelslike_c": 19.0,
                                "condition": {
                                    "text": "Clear",
                                    "icon": "//cdn.weatherapi.com/weather/64x64/day/113.png"
                                },
                                "wind_kph": 12.0,
                                "humidity": 50,
                                "chance_of_rain": 10,
                                "is_day": 1
                            }
                        ]
                    }
                ]
            }
        }
        """;
}
