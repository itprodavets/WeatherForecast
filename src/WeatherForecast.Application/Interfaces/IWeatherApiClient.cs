using WeatherForecast.Domain.Entities;
using WeatherForecast.Domain.ValueObjects;

namespace WeatherForecast.Application.Interfaces;

public interface IWeatherApiClient
{
    Task<CurrentWeather> GetCurrentWeatherAsync(Coordinates coordinates, CancellationToken cancellationToken = default);
    Task<(Location Location, List<DayForecast> Days)> GetForecastAsync(Coordinates coordinates, int days, CancellationToken cancellationToken = default);
}
