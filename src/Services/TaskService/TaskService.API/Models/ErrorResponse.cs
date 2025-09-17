using System.Text.Json.Serialization;

namespace TaskService.API.Models;

/// <summary>
/// Represents a standardized error response
/// </summary>
public class ErrorResponse
{
    [JsonPropertyName("code")]
    public string Code { get; }

    [JsonPropertyName("message")]
    public string Message { get; }

    [JsonPropertyName("details")]
    public string Details { get; }

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; }

    [JsonPropertyName("traceId")]
    public string? TraceId { get; }

    public ErrorResponse(string code, string message, string details = "", string? traceId = null)
    {
        Code = code;
        Message = message;
        Details = details;
        Timestamp = DateTime.UtcNow;
        TraceId = traceId;
    }

    public static ErrorResponse FromException(TaskService.Domain.Common.Exceptions.TaskServiceException exception, string? traceId = null)
    {
        return new ErrorResponse(
            exception.Code,
            exception.Message,
            exception.Details,
            traceId
        );
    }

    public static ErrorResponse FromException(Exception exception, string? traceId = null)
    {
        return new ErrorResponse(
            "INTERNAL_SERVER_ERROR",
            "An unexpected error occurred",
            exception.Message,
            traceId
        );
    }
}