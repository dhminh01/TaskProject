using HotChocolate;

namespace TaskService.API.GraphQL.Filters;

public class GraphQLErrorFilter : IErrorFilter
{
    public IError OnError(IError error)
    {
        // Get the error code from extensions if it exists
        var code = error.Extensions?.GetValueOrDefault("code")?.ToString();

        // Map error codes to HTTP status codes
        var statusCode = code switch
        {
            "VALIDATION_ERROR" => 400, // Bad Request
            "NOT_FOUND_ERROR" => 404,  // Not Found
            "UNAUTHORIZED" => 401,      // Unauthorized
            "FORBIDDEN" => 403,         // Forbidden
            "INTERNAL_ERROR" => 500,    // Internal Server Error
            _ => 500                    // Default to 500 for unknown errors
        };

        // Create a new dictionary with existing extensions plus our status code
        var extensions = new Dictionary<string, object?>(error.Extensions ?? new Dictionary<string, object?>())
        {
            ["statusCode"] = statusCode
        };

        // Return error with status code added to extensions
        return error.WithMessage(error.Exception?.Message ?? error.Message)
                   .WithExtensions(extensions);
    }
}
