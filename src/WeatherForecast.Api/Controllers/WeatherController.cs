using MediatR;
using Microsoft.AspNetCore.Mvc;
using WeatherForecast.Application.DTOs;
using WeatherForecast.Application.Queries.GetWeatherDashboard;

namespace WeatherForecast.Api.Controllers;

/// <summary>
/// Weather data endpoints for Moscow.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class WeatherController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Returns aggregated weather dashboard: current conditions, hourly forecast
    /// (remaining hours today + all hours tomorrow), and 3-day forecast for Moscow.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Weather dashboard data.</returns>
    /// <response code="200">Weather data retrieved successfully.</response>
    /// <response code="503">Weather API is unavailable.</response>
    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(WeatherDashboardResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    [ResponseCache(Duration = 60)]
    public async Task<ActionResult<WeatherDashboardResponse>> GetDashboard(
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetWeatherDashboardQuery(), cancellationToken);
        return Ok(result);
    }
}
