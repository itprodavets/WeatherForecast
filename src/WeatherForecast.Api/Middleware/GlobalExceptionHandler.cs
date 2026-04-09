using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace WeatherForecast.Api.Middleware;

public sealed partial class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // Unwrap AggregateException to get the real cause (e.g. from Task.WhenAll)
        var actualException = exception is AggregateException aggregate
            ? aggregate.GetBaseException()
            : exception;

        LogUnhandledException(logger, actualException.Message, actualException);

        var (statusCode, title, detail) = actualException switch
        {
            HttpRequestException => ((int)HttpStatusCode.ServiceUnavailable,
                "Weather service is temporarily unavailable",
                "The upstream weather API is not responding. Please try again later."),
            TaskCanceledException => ((int)HttpStatusCode.ServiceUnavailable,
                "Request timed out",
                "The request to the weather service timed out. Please try again."),
            InvalidOperationException => ((int)HttpStatusCode.BadGateway,
                "Invalid response from weather service",
                "Received an unexpected response from the weather API."),
            _ => ((int)HttpStatusCode.InternalServerError,
                "An unexpected error occurred",
                "An internal server error occurred. Please try again later.")
        };

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path
        };

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Unhandled exception occurred: {Message}")]
    private static partial void LogUnhandledException(ILogger logger, string message, Exception ex);
}
