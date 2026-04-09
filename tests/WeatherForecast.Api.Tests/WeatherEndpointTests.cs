using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using WeatherForecast.Application.DTOs;
using WeatherForecast.Application.Interfaces;
using WeatherForecast.Domain.ValueObjects;

namespace WeatherForecast.Api.Tests;

public class WeatherEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly IWeatherApiClient _weatherApiClient;

    public WeatherEndpointTests(WebApplicationFactory<Program> factory)
    {
        _weatherApiClient = Substitute.For<IWeatherApiClient>();

        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Replace real weather client with mock
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IWeatherApiClient));
                if (descriptor is not null)
                    services.Remove(descriptor);

                services.AddSingleton(_weatherApiClient);
            });
        }).CreateClient();
    }

    [Fact]
    public async Task GetDashboard_ReturnsOk_WithWeatherData()
    {
        // Arrange
        SetupMockWeatherApi();

        // Act
        var response = await _client.GetAsync("/api/weather/dashboard");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<WeatherDashboardResponse>();
        result.Should().NotBeNull();
        result!.Location.Name.Should().Be("Moscow");
        result.Current.TempCelsius.Should().Be(22.0);
    }

    [Fact]
    public async Task GetDashboard_ReturnsJsonContentType()
    {
        // Arrange
        SetupMockWeatherApi();

        // Act
        var response = await _client.GetAsync("/api/weather/dashboard");

        // Assert
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/json");
    }

    [Fact]
    public async Task GetDashboard_IncludesResponseTimeHeader()
    {
        // Arrange
        SetupMockWeatherApi();

        // Act
        var response = await _client.GetAsync("/api/weather/dashboard");

        // Assert
        response.Headers.Contains("X-Response-Time").Should().BeTrue();
    }

    [Fact]
    public async Task HealthCheck_ReturnsHealthy()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetDashboard_WhenApiThrows_ReturnsServiceUnavailable()
    {
        // Arrange
        _weatherApiClient.GetCurrentWeatherAsync(Arg.Any<Coordinates>(), Arg.Any<CancellationToken>())
            .Returns<(Domain.Entities.Location, Domain.Entities.CurrentWeather)>(
                x => throw new HttpRequestException("Weather API is down"));

        _weatherApiClient.GetForecastAsync(Arg.Any<Coordinates>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns<(Domain.Entities.Location, List<Domain.Entities.DayForecast>)>(
                x => throw new HttpRequestException("Weather API is down"));

        // Act
        var response = await _client.GetAsync("/api/weather/dashboard");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
    }

    private void SetupMockWeatherApi()
    {
        var location = new Domain.Entities.Location
        {
            Name = "Moscow",
            Region = "Moscow City",
            Country = "Russia",
            Latitude = 55.75,
            Longitude = 37.62,
            TimeZone = "Europe/Moscow",
            LocalTime = DateTime.UtcNow
        };

        var current = new Domain.Entities.CurrentWeather
        {
            Temperature = new Temperature(22.0, 20.0),
            Wind = new Wind(15.0, "NW", 315),
            Humidity = 55,
            PressureMb = 1015,
            CloudCover = 40,
            UvIndex = 4,
            VisibilityKm = 10,
            ConditionText = "Partly cloudy",
            ConditionIconUrl = "https://cdn.weatherapi.com/weather/64x64/day/116.png",
            IsDay = true,
            LastUpdated = DateTime.UtcNow
        };

        var days = new List<Domain.Entities.DayForecast>
        {
            new()
            {
                Date = DateOnly.FromDateTime(DateTime.UtcNow),
                TemperatureRange = new TemperatureRange(25, 15, 20),
                MaxWind = new Wind(20, string.Empty, 0),
                AvgHumidity = 60,
                ChanceOfRain = 30,
                TotalPrecipitationMm = 2.5,
                UvIndex = 5,
                ConditionText = "Partly cloudy",
                ConditionIconUrl = "https://cdn.weatherapi.com/weather/64x64/day/116.png",
                Hours = [
                    new()
                    {
                        Time = DateTime.UtcNow.AddHours(1),
                        Temperature = new Temperature(22, 20),
                        ConditionText = "Clear",
                        ConditionIconUrl = "https://cdn.weatherapi.com/weather/64x64/day/113.png",
                        Wind = new Wind(10, string.Empty, 0),
                        Humidity = 50,
                        ChanceOfRain = 0,
                        IsDay = true
                    }
                ]
            }
        };

        _weatherApiClient.GetCurrentWeatherAsync(Arg.Any<Coordinates>(), Arg.Any<CancellationToken>())
            .Returns((location, current));

        _weatherApiClient.GetForecastAsync(Arg.Any<Coordinates>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns((location, days));
    }
}
