using System.Globalization;
using WeatherForecast.Domain.Entities;
using WeatherForecast.Domain.ValueObjects;
using WeatherForecast.Infrastructure.WeatherApi.Models;

namespace WeatherForecast.Infrastructure.WeatherApi;

internal static class WeatherApiMapper
{
    private const string DateTimeFormat = "yyyy-MM-dd H:mm";
    private const string DateFormat = "yyyy-MM-dd";
    private const string HourTimeFormat = "yyyy-MM-dd HH:mm";

    public static CurrentWeather MapToCurrent(CurrentModel model) => new()
    {
        Temperature = new Temperature(model.TempC, model.FeelsLikeC),
        Wind = new Wind(model.WindKph, model.WindDir, model.WindDegree),
        Humidity = model.Humidity,
        PressureMb = model.PressureMb,
        CloudCover = model.Cloud,
        UvIndex = model.Uv,
        VisibilityKm = model.VisKm,
        ConditionText = model.Condition.Text,
        ConditionIconUrl = NormalizeIconUrl(model.Condition.Icon),
        IsDay = model.IsDay == 1,
        LastUpdated = ParseDateTime(model.LastUpdated)
    };

    public static Location MapToLocation(LocationModel model) => new()
    {
        Name = model.Name,
        Region = model.Region,
        Country = model.Country,
        Latitude = model.Lat,
        Longitude = model.Lon,
        TimeZone = model.TzId,
        LocalTime = ParseDateTime(model.LocalTime)
    };

    public static DayForecast MapToDay(ForecastDayModel model) => new()
    {
        Date = DateOnly.ParseExact(model.Date, DateFormat, CultureInfo.InvariantCulture),
        MaxTempCelsius = model.Day.MaxTempC,
        MinTempCelsius = model.Day.MinTempC,
        AvgTempCelsius = model.Day.AvgTempC,
        MaxWindSpeedKph = model.Day.MaxWindKph,
        AvgHumidity = model.Day.AvgHumidity,
        ChanceOfRain = model.Day.DailyChanceOfRain,
        TotalPrecipitationMm = model.Day.TotalPrecipMm,
        UvIndex = model.Day.Uv,
        ConditionText = model.Day.Condition.Text,
        ConditionIconUrl = NormalizeIconUrl(model.Day.Condition.Icon),
        Hours = model.Hour.Select(MapToHour).ToList()
    };

    public static HourForecast MapToHour(HourModel model) => new()
    {
        Time = ParseDateTime(model.Time),
        TempCelsius = model.TempC,
        FeelsLikeCelsius = model.FeelsLikeC,
        ConditionText = model.Condition.Text,
        ConditionIconUrl = NormalizeIconUrl(model.Condition.Icon),
        WindSpeedKph = model.WindKph,
        Humidity = model.Humidity,
        ChanceOfRain = model.ChanceOfRain,
        IsDay = model.IsDay == 1
    };

    private static DateTime ParseDateTime(string value) =>
        DateTime.TryParseExact(value, HourTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt)
            ? dt
            : DateTime.TryParseExact(value, DateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt)
                ? dt
                : DateTime.Parse(value, CultureInfo.InvariantCulture);

    private static string NormalizeIconUrl(string icon) =>
        icon.StartsWith("//", StringComparison.Ordinal) ? $"https:{icon}" : icon;
}
