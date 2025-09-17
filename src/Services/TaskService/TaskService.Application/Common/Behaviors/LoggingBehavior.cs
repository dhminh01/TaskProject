using System.Diagnostics;
using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Logging;
using TaskService.Domain.Common.Exceptions;

namespace TaskService.Application.Common.Behaviors;

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestType = typeof(TRequest).Name;
        var stopwatch = Stopwatch.StartNew();
        var correlationId = Guid.NewGuid();

        try
        {
            // Log the request with additional context
            var requestProperties = GetLoggableProperties(request);
            _logger.LogInformation(
                "Starting request {RequestType} [CorrelationId: {CorrelationId}]. Request details: {@RequestProperties}",
                requestType,
                correlationId,
                requestProperties);

            var response = await next();
            stopwatch.Stop();

            // Log successful response with timing
            _logger.LogInformation(
                "Completed request {RequestType} [CorrelationId: {CorrelationId}] in {ElapsedMilliseconds}ms",
                requestType,
                correlationId,
                stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            LogException(ex, requestType, correlationId, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    private void LogException(Exception ex, string requestType, Guid correlationId, long elapsedMilliseconds)
    {
        var (level, message) = ex switch
        {
            TaskValidationException ve => (LogLevel.Warning, $"Validation error in {requestType}: {ve.Message}"),
            TaskServiceException tse => (LogLevel.Warning, $"Business error in {requestType}: [{tse.Code}] {tse.Message}"),
            _ => (LogLevel.Error, $"Unhandled exception in {requestType}: {ex.Message}")
        };

        _logger.Log(
            level,
            ex,
            "{Message} [CorrelationId: {CorrelationId}] after {ElapsedMilliseconds}ms",
            message,
            correlationId,
            elapsedMilliseconds);

        // Log stack trace for unexpected errors
        if (level == LogLevel.Error)
        {
            _logger.LogDebug(
                "Detailed stack trace for {RequestType} [CorrelationId: {CorrelationId}]: {StackTrace}",
                requestType,
                correlationId,
                ex.StackTrace);
        }
    }

    private static Dictionary<string, object?> GetLoggableProperties(TRequest request)
    {
        try
        {
            // Convert request to dictionary, excluding sensitive properties
            var properties = JsonSerializer.Deserialize<Dictionary<string, object?>>(
                JsonSerializer.Serialize(request))!;

            // Remove sensitive information
            var sensitiveProps = new[] { "password", "token", "secret", "key" };
            foreach (var prop in sensitiveProps)
            {
                if (properties.ContainsKey(prop))
                {
                    properties[prop] = "***REDACTED***";
                }
            }

            return properties;
        }
        catch
        {
            // Fallback if serialization fails
            return new Dictionary<string, object?>
            {
                { "requestType", typeof(TRequest).Name }
            };
        }
    }
}