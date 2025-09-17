using System.Net;
using TaskService.API.Models;
using TaskService.Domain.Common.Exceptions;
using FluentValidation;
using System.Text.Json;

namespace TaskService.API.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ErrorHandlingMiddleware(
        RequestDelegate next,
        ILogger<ErrorHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception error)
        {
            var response = context.Response;
            response.ContentType = "application/json";

            var errorResponse = HandleException(error, context);
            response.StatusCode = errorResponse.StatusCode;

            var result = JsonSerializer.Serialize(errorResponse.Error);
            await response.WriteAsync(result);
        }
    }

    private (int StatusCode, ErrorResponse Error) HandleException(Exception exception, HttpContext context)
    {
        var traceId = context.TraceIdentifier;
        int statusCode;
        ErrorResponse errorResponse;

        switch (exception)
        {
            case TaskServiceException tse:
                statusCode = tse.StatusCode;
                errorResponse = ErrorResponse.FromException(tse, traceId);
                LogError(tse, LogLevel.Warning);
                break;

            case ValidationException ve:
                statusCode = (int)HttpStatusCode.BadRequest;
                var details = string.Join("; ", ve.Errors.Select(x => x.ErrorMessage));
                errorResponse = new ErrorResponse(
                    "VALIDATION_ERROR",
                    "One or more validation errors occurred",
                    details,
                    traceId
                );
                LogError(ve, LogLevel.Warning);
                break;

            default:
                statusCode = (int)HttpStatusCode.InternalServerError;
                errorResponse = new ErrorResponse(
                    "INTERNAL_SERVER_ERROR",
                    "An unexpected error occurred",
                    _environment.IsDevelopment() ? exception.ToString() : "Contact support for more information",
                    traceId
                );
                LogError(exception, LogLevel.Error);
                break;
        }

        return (statusCode, errorResponse);
    }

    private void LogError(Exception exception, LogLevel level)
    {
        var errorMessage = exception switch
        {
            TaskServiceException tse => $"[{tse.Code}] {tse.Message} - Details: {tse.Details}",
            ValidationException ve => $"Validation error: {string.Join("; ", ve.Errors.Select(x => x.ErrorMessage))}",
            _ => exception.Message
        };

        _logger.Log(level, exception, "Error handling request: {ErrorMessage}", errorMessage);
    }
}
