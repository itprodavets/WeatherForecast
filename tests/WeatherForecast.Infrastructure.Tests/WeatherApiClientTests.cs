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
    private static readonly Coordinates Moscow = new(55.7558, 37.6173);

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
    public async Task GetCurrentWeatherAsync_ReturnsLocationAndCurrentWeather()
    {
        // Arrange
        _server.Given(Request.Create().WithPath("/v1/current.json").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody(CurrentJson));

        // Act
        var (location, current) = await _client.GetCurrentWeatherAsync(Moscow);

        // Assert
        location.Should().NotBeNull();
        location.Name.Should().Be("Moscow");
        location.TimeZone.Should().Be("Europe/Moscow");

        current.Should().NotBeNull();
        current.Temperature.Celsius.Should().Be(22.0);
        current.Temperature.FeelsLike.Should().Be(21.0);
        current.Humidity.Should().Be(55);
        current.Wind.SpeedKph.Should().Be(15.1);
        current.Wind.Direction.Should().Be("NW");
        current.ConditionText.Should().Be("Partly cloudy");
        current.IsDay.Should().BeTrue();
    }

    [Fact]
    public async Task GetCurrentWeatherAsync_WhenApiReturnsError_ThrowsHttpRequestException()
    {
        // Arrange
        _server.Given(Request.Create().WithPath("/v1/current.json").UsingGet())
            .RespondWith(Response.Create().WithStatusCode(HttpStatusCode.InternalServerError));

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(
            () => _client.GetCurrentWeatherAsync(Moscow));
    }

    [Fact]
    public async Task GetForecastAsync_ReturnsForecastWithLocationAndDays()
    {
        // Arrange
        _server.Given(Request.Create().WithPath("/v1/forecast.json").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody(ForecastJson));

        // Act
        var (location, days) = await _client.GetForecastAsync(Moscow, 3);

        // Assert
        location.Should().NotBeNull();
        location.Name.Should().Be("Moscow");

        days.Should().HaveCount(1);
        days[0].TemperatureRange.MaxCelsius.Should().Be(25.0);
        days[0].TemperatureRange.MinCelsius.Should().Be(14.0);
        days[0].Hours.Should().HaveCount(1);
        days[0].Hours[0].Temperature.Celsius.Should().Be(20.0);
    }

    [Fact]
    public async Task GetForecastAsync_WhenApiReturnsError_ThrowsHttpRequestException()
    {
        // Arrange
        _server.Given(Request.Create().WithPath("/v1/forecast.json").UsingGet())
            .RespondWith(Response.Create().WithStatusCode(HttpStatusCode.InternalServerError));

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(
            () => _client.GetForecastAsync(Moscow, 3));
    }

    [Fact]
    public async Task GetCurrentWeatherAsync_CorrectlyMapsIconUrl()
    {
        // Arrange
        _server.Given(Request.Create().WithPath("/v1/current.json").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody(CurrentJson));

        // Act
        var (_, current) = await _client.GetCurrentWeatherAsync(Moscow);

        // Assert — icon URL should be normalized with https:
        current.ConditionIconUrl.Should().StartWith("https://");
    }

    [Fact]
    public async Task GetForecastAsync_CorrectlyMapsIconUrl()
    {
        // Arrange
        _server.Given(Request.Create().WithPath("/v1/forecast.json").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody(ForecastJson));

        // Act
        var (_, days) = await _client.GetForecastAsync(Moscow, 3);

        // Assert
        days[0].ConditionIconUrl.Should().StartWith("https://");
    }

    [Fact]
    public async Task GetCurrentWeatherAsync_SendsCorrectQueryParameters()
    {
        // Arrange
        _server.Given(Request.Create().WithPath("/v1/current.json").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody(CurrentJson));

        // Act
        await _client.GetCurrentWeatherAsync(Moscow);

        // Assert
        var requests = _server.LogEntries;
        requests.Should().ContainSingle();

        var request = requests[0];
        var query = request.RequestMessage!.RawQuery ?? string.Empty;
        query.Should().Contain("key=test-key");
        query.Should().Contain("q=55.7558,37.6173");
    }

    [Fact]
    public async Task GetForecastAsync_SendsCorrectQueryParameters()
    {
        // Arrange
        _server.Given(Request.Create().WithPath("/v1/forecast.json").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody(ForecastJson));

        // Act
        await _client.GetForecastAsync(Moscow, 3);

        // Assert
        var requests = _server.LogEntries;
        requests.Should().ContainSingle();

        var request = requests[0];
        var query = request.RequestMessage!.RawQuery ?? string.Empty;
        query.Should().Contain("key=test-key");
        query.Should().Contain("q=55.7558,37.6173");
        query.Should().Contain("days=3");
    }

    public void Dispose()
    {
        _server.Dispose();
        GC.SuppressFinalize(this);
    }

    private const string CurrentJson = """
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
