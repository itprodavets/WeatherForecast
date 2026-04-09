using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using WeatherForecast.Application.Configuration;
using WeatherForecast.Application.DTOs;
using WeatherForecast.Application.Interfaces;
using WeatherForecast.Application.Queries.GetWeatherDashboard;
using WeatherForecast.Domain.Entities;
using WeatherForecast.Domain.ValueObjects;

namespace WeatherForecast.Application.Tests;

public class GetWeatherDashboardQueryHandlerTests
{
    private readonly IWeatherApiClient _weatherApiClient = Substitute.For<IWeatherApiClient>();
    private readonly ICacheService _cacheService = Substitute.For<ICacheService>();
    private readonly ILogger<GetWeatherDashboardQueryHandler> _logger = Substitute.For<ILogger<GetWeatherDashboardQueryHandler>>();
    private readonly GetWeatherDashboardQueryHandler _handler;

    public GetWeatherDashboardQueryHandlerTests()
    {
        _handler = new GetWeatherDashboardQueryHandler(_weatherApiClient, _cacheService, _logger);
    }

    [Fact]
    public async Task Handle_WhenCacheHit_ReturnsCachedData()
    {
        // Arrange
        var cachedResponse = CreateSampleResponse();
        _cacheService.GetOrCreateAsync(
                Arg.Any<string>(),
                Arg.Any<Func<CancellationToken, Task<WeatherDashboardResponse>>>(),
                Arg.Any<TimeSpan>(),
                Arg.Any<CancellationToken>())
            .Returns(cachedResponse);

        // Act
        var result = await _handler.Handle(new GetWeatherDashboardQuery(), CancellationToken.None);

        // Assert
        result.Should().BeSameAs(cachedResponse);
    }

    [Fact]
    public async Task Handle_WhenCacheMiss_FetchesFromBothApisAndCaches()
    {
        // Arrange — simulate cache miss by executing the factory
        SetupMockApis(CreateSampleLocation(), CreateSampleCurrentWeather(), CreateSampleDays());
        SetupCacheToExecuteFactory();

        // Act
        var result = await _handler.Handle(new GetWeatherDashboardQuery(), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Location.Name.Should().Be("Moscow");
        result.Current.TempCelsius.Should().Be(22.5);
        result.DailyForecast.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_CallsBothCurrentAndForecastApis()
    {
        // Arrange
        SetupMockApis(CreateSampleLocation(), CreateSampleCurrentWeather(), CreateSampleDays());
        SetupCacheToExecuteFactory();

        // Act
        await _handler.Handle(new GetWeatherDashboardQuery(), CancellationToken.None);

        // Assert — both API methods called with correct coordinates
        var expectedCoordinates = WeatherDefaults.DefaultCoordinates;

        await _weatherApiClient.Received(1).GetCurrentWeatherAsync(
            Arg.Is<Coordinates>(c => c == expectedCoordinates),
            Arg.Any<CancellationToken>());

        await _weatherApiClient.Received(1).GetForecastAsync(
            Arg.Is<Coordinates>(c => c == expectedCoordinates),
            WeatherDefaults.ForecastDays,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_FiltersHourlyForecast_OnlyRemainingHoursAndNextDay()
    {
        // Arrange
        var now = new DateTime(2026, 4, 9, 14, 0, 0);
        var location = CreateSampleLocation(now);

        var hours = new List<HourForecast>
        {
            // Past hours (should be filtered out)
            CreateHour(new DateTime(2026, 4, 9, 10, 0, 0)),
            CreateHour(new DateTime(2026, 4, 9, 12, 0, 0)),
            // Current + future hours (should be included)
            CreateHour(new DateTime(2026, 4, 9, 14, 0, 0)),
            CreateHour(new DateTime(2026, 4, 9, 18, 0, 0)),
            CreateHour(new DateTime(2026, 4, 9, 22, 0, 0)),
        };

        var tomorrowHours = new List<HourForecast>
        {
            CreateHour(new DateTime(2026, 4, 10, 6, 0, 0)),
            CreateHour(new DateTime(2026, 4, 10, 12, 0, 0)),
            CreateHour(new DateTime(2026, 4, 10, 18, 0, 0)),
        };

        // Day after tomorrow — these hours should NOT be included
        var dayAfterTomorrowHours = new List<HourForecast>
        {
            CreateHour(new DateTime(2026, 4, 11, 6, 0, 0)),
            CreateHour(new DateTime(2026, 4, 11, 12, 0, 0)),
        };

        var days = new List<DayForecast>
        {
            CreateDay(new DateOnly(2026, 4, 9), hours),
            CreateDay(new DateOnly(2026, 4, 10), tomorrowHours),
            CreateDay(new DateOnly(2026, 4, 11), dayAfterTomorrowHours),
        };

        SetupMockApis(location, CreateSampleCurrentWeather(), days);
        SetupCacheToExecuteFactory();

        // Act
        var result = await _handler.Handle(new GetWeatherDashboardQuery(), CancellationToken.None);

        // Assert — past hours filtered out, today remaining + tomorrow included, day after tomorrow excluded
        result.HourlyForecast.Should().HaveCount(6); // 3 remaining today + 3 tomorrow (NOT day after tomorrow)
    }

    private void SetupCacheToExecuteFactory()
    {
        _cacheService.GetOrCreateAsync(
                Arg.Any<string>(),
                Arg.Any<Func<CancellationToken, Task<WeatherDashboardResponse>>>(),
                Arg.Any<TimeSpan>(),
                Arg.Any<CancellationToken>())
            .Returns(async callInfo =>
            {
                var factory = callInfo.ArgAt<Func<CancellationToken, Task<WeatherDashboardResponse>>>(1);
                var ct = callInfo.ArgAt<CancellationToken>(3);
                return await factory(ct);
            });
    }

    private void SetupMockApis(Location location, CurrentWeather current, List<DayForecast> days)
    {
        _weatherApiClient.GetCurrentWeatherAsync(Arg.Any<Coordinates>(), Arg.Any<CancellationToken>())
            .Returns((location, current));

        _weatherApiClient.GetForecastAsync(Arg.Any<Coordinates>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns((location, days));
    }

    private static WeatherDashboardResponse CreateSampleResponse() => new()
    {
        Location = new LocationDto { Name = "Moscow", Region = "Moscow", Country = "Russia", TimeZone = "Europe/Moscow", LocalTime = DateTime.Now },
        Current = new CurrentWeatherDto { TempCelsius = 20, FeelsLikeCelsius = 18, Humidity = 60, PressureMb = 1013, WindSpeedKph = 10, WindDirection = "N", CloudCover = 50, UvIndex = 3, VisibilityKm = 10, ConditionText = "Partly cloudy", ConditionIconUrl = "https://cdn.weatherapi.com/weather/64x64/day/116.png", IsDay = true, LastUpdated = DateTime.Now },
        HourlyForecast = [],
        DailyForecast = []
    };

    private static CurrentWeather CreateSampleCurrentWeather() => new()
    {
        Temperature = new Temperature(22.5, 20.0),
        Wind = new Wind(15.0, "NW", 315),
        Humidity = 55,
        PressureMb = 1015,
        CloudCover = 40,
        UvIndex = 4,
        VisibilityKm = 10,
        ConditionText = "Partly cloudy",
        ConditionIconUrl = "https://cdn.weatherapi.com/weather/64x64/day/116.png",
        IsDay = true,
        LastUpdated = DateTime.Now
    };

    private static Location CreateSampleLocation(DateTime? localTime = null) => new()
    {
        Name = "Moscow",
        Region = "Moscow City",
        Country = "Russia",
        Latitude = 55.7558,
        Longitude = 37.6173,
        TimeZone = "Europe/Moscow",
        LocalTime = localTime ?? DateTime.Now
    };

    private static List<DayForecast> CreateSampleDays() =>
    [
        CreateDay(DateOnly.FromDateTime(DateTime.Now), [CreateHour(DateTime.Now.AddHours(1))])
    ];

    private static DayForecast CreateDay(DateOnly date, List<HourForecast> hours) => new()
    {
        Date = date,
        TemperatureRange = new TemperatureRange(25, 15, 20),
        MaxWind = new Wind(20, string.Empty, 0),
        AvgHumidity = 60,
        ChanceOfRain = 30,
        TotalPrecipitationMm = 2.5,
        UvIndex = 5,
        ConditionText = "Partly cloudy",
        ConditionIconUrl = "https://cdn.weatherapi.com/weather/64x64/day/116.png",
        Hours = hours
    };

    private static HourForecast CreateHour(DateTime time) => new()
    {
        Time = time,
        Temperature = new Temperature(20, 18),
        ConditionText = "Clear",
        ConditionIconUrl = "https://cdn.weatherapi.com/weather/64x64/day/113.png",
        Wind = new Wind(10, string.Empty, 0),
        Humidity = 50,
        ChanceOfRain = 0,
        IsDay = true
    };
}
