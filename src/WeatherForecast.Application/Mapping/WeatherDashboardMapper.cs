using System.Globalization;
using WeatherForecast.Application.DTOs;
using WeatherForecast.Domain.Entities;

namespace WeatherForecast.Application.Mapping;

public static class WeatherDashboardMapper
{
    public static LocationDto ToDto(this Location location) => new()
    {
        Name = location.Name,
        Region = location.Region,
        Country = location.Country,
        TimeZone = location.TimeZone,
        LocalTime = location.LocalTime
    };

    public static CurrentWeatherDto ToDto(this CurrentWeather current) => new()
    {
        TempCelsius = current.Temperature.Celsius,
        FeelsLikeCelsius = current.Temperature.FeelsLike,
        Humidity = current.Humidity,
        PressureMb = current.PressureMb,
        WindSpeedKph = current.Wind.SpeedKph,
        WindDirection = current.Wind.Direction,
        CloudCover = current.CloudCover,
        UvIndex = current.UvIndex,
        VisibilityKm = current.VisibilityKm,
        ConditionText = current.ConditionText,
        ConditionIconUrl = current.ConditionIconUrl,
        IsDay = current.IsDay,
        LastUpdated = current.LastUpdated
    };

    public static HourForecastDto ToDto(this HourForecast hour) => new()
    {
        Time = hour.Time,
        TempCelsius = hour.Temperature.Celsius,
        FeelsLikeCelsius = hour.Temperature.FeelsLike,
        ConditionText = hour.ConditionText,
        ConditionIconUrl = hour.ConditionIconUrl,
        WindSpeedKph = hour.Wind.SpeedKph,
        Humidity = hour.Humidity,
        ChanceOfRain = hour.ChanceOfRain,
        IsDay = hour.IsDay
    };

    public static DayForecastDto ToDto(this DayForecast day) => new()
    {
        Date = day.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
        MaxTempCelsius = day.TemperatureRange.MaxCelsius,
        MinTempCelsius = day.TemperatureRange.MinCelsius,
        AvgTempCelsius = day.TemperatureRange.AvgCelsius,
        MaxWindSpeedKph = day.MaxWind.SpeedKph,
        AvgHumidity = day.AvgHumidity,
        ChanceOfRain = day.ChanceOfRain,
        TotalPrecipitationMm = day.TotalPrecipitationMm,
        UvIndex = day.UvIndex,
        ConditionText = day.ConditionText,
        ConditionIconUrl = day.ConditionIconUrl
    };

    public static WeatherDashboardResponse ToResponse(
        Location location,
        CurrentWeather current,
        List<HourForecast> hourlyForecast,
        List<DayForecast> dailyForecast) => new()
    {
        Location = location.ToDto(),
        Current = current.ToDto(),
        HourlyForecast = hourlyForecast.Select(h => h.ToDto()).ToList(),
        DailyForecast = dailyForecast.Select(d => d.ToDto()).ToList()
    };
}
