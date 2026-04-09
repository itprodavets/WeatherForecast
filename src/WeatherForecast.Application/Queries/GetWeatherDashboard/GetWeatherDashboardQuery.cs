using System.Globalization;
using MediatR;
using WeatherForecast.Application.Configuration;
using WeatherForecast.Application.DTOs;
using WeatherForecast.Application.Interfaces;
using WeatherForecast.Domain.ValueObjects;

namespace WeatherForecast.Application.Queries.GetWeatherDashboard;

public sealed record GetWeatherDashboardQuery : IRequest<WeatherDashboardResponse>, ICacheable
{
    private static string BuildCacheKey(Coordinates coordinates) =>
        string.Create(CultureInfo.InvariantCulture, $"weather:dashboard:{coordinates.Latitude}:{coordinates.Longitude}");

    public string CacheKey => BuildCacheKey(WeatherDefaults.DefaultCoordinates);

    public TimeSpan CacheDuration => TimeSpan.FromMinutes(5);
}
