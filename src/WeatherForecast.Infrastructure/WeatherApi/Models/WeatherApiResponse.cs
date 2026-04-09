using System.Text.Json.Serialization;

namespace WeatherForecast.Infrastructure.WeatherApi.Models;

public sealed class CurrentResponse
{
    [JsonPropertyName("location")]
    public required LocationModel Location { get; set; }

    [JsonPropertyName("current")]
    public required CurrentModel Current { get; set; }
}

public sealed class ForecastResponse
{
    [JsonPropertyName("location")]
    public required LocationModel Location { get; set; }

    [JsonPropertyName("forecast")]
    public required ForecastModel Forecast { get; set; }
}

public sealed class LocationModel
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("region")]
    public required string Region { get; set; }

    [JsonPropertyName("country")]
    public required string Country { get; set; }

    [JsonPropertyName("lat")]
    public double Lat { get; set; }

    [JsonPropertyName("lon")]
    public double Lon { get; set; }

    [JsonPropertyName("tz_id")]
    public required string TzId { get; set; }

    [JsonPropertyName("localtime")]
    public required string LocalTime { get; set; }
}

public sealed class CurrentModel
{
    [JsonPropertyName("temp_c")]
    public double TempC { get; set; }

    [JsonPropertyName("feelslike_c")]
    public double FeelsLikeC { get; set; }

    [JsonPropertyName("humidity")]
    public int Humidity { get; set; }

    [JsonPropertyName("pressure_mb")]
    public double PressureMb { get; set; }

    [JsonPropertyName("wind_kph")]
    public double WindKph { get; set; }

    [JsonPropertyName("wind_dir")]
    public required string WindDir { get; set; }

    [JsonPropertyName("wind_degree")]
    public int WindDegree { get; set; }

    [JsonPropertyName("cloud")]
    public int Cloud { get; set; }

    [JsonPropertyName("uv")]
    public double Uv { get; set; }

    [JsonPropertyName("vis_km")]
    public double VisKm { get; set; }

    [JsonPropertyName("condition")]
    public required ConditionModel Condition { get; set; }

    [JsonPropertyName("is_day")]
    public int IsDay { get; set; }

    [JsonPropertyName("last_updated")]
    public required string LastUpdated { get; set; }
}

public sealed class ConditionModel
{
    [JsonPropertyName("text")]
    public required string Text { get; set; }

    [JsonPropertyName("icon")]
    public required string Icon { get; set; }
}

public sealed class ForecastModel
{
    [JsonPropertyName("forecastday")]
    public required List<ForecastDayModel> ForecastDay { get; set; }
}

public sealed class ForecastDayModel
{
    [JsonPropertyName("date")]
    public required string Date { get; set; }

    [JsonPropertyName("day")]
    public required DayModel Day { get; set; }

    [JsonPropertyName("hour")]
    public required List<HourModel> Hour { get; set; }
}

public sealed class DayModel
{
    [JsonPropertyName("maxtemp_c")]
    public double MaxTempC { get; set; }

    [JsonPropertyName("mintemp_c")]
    public double MinTempC { get; set; }

    [JsonPropertyName("avgtemp_c")]
    public double AvgTempC { get; set; }

    [JsonPropertyName("maxwind_kph")]
    public double MaxWindKph { get; set; }

    [JsonPropertyName("avghumidity")]
    public int AvgHumidity { get; set; }

    [JsonPropertyName("daily_chance_of_rain")]
    public int DailyChanceOfRain { get; set; }

    [JsonPropertyName("totalprecip_mm")]
    public double TotalPrecipMm { get; set; }

    [JsonPropertyName("uv")]
    public double Uv { get; set; }

    [JsonPropertyName("condition")]
    public required ConditionModel Condition { get; set; }
}

public sealed class HourModel
{
    [JsonPropertyName("time")]
    public required string Time { get; set; }

    [JsonPropertyName("temp_c")]
    public double TempC { get; set; }

    [JsonPropertyName("feelslike_c")]
    public double FeelsLikeC { get; set; }

    [JsonPropertyName("condition")]
    public required ConditionModel Condition { get; set; }

    [JsonPropertyName("wind_kph")]
    public double WindKph { get; set; }

    [JsonPropertyName("humidity")]
    public int Humidity { get; set; }

    [JsonPropertyName("chance_of_rain")]
    public int ChanceOfRain { get; set; }

    [JsonPropertyName("is_day")]
    public int IsDay { get; set; }
}
