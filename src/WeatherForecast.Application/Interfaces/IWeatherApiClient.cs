using WeatherForecast.Domain.Entities;
using WeatherForecast.Domain.ValueObjects;

namespace WeatherForecast.Application.Interfaces;

public interface IWeatherApiClient
{
    Task<(Location Location, CurrentWeather Current, List<DayForecast> Days)> GetForecastAsync(
        Coordinates coordinates, int days, CancellationToken cancellationToken = default);
}
