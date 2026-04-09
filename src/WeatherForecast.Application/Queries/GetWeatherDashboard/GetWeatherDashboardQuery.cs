using MediatR;
using WeatherForecast.Application.DTOs;

namespace WeatherForecast.Application.Queries.GetWeatherDashboard;

public sealed record GetWeatherDashboardQuery : IRequest<WeatherDashboardResponse>;
