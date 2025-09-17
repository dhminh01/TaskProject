namespace TaskService.Domain.Common.Exceptions;

/// <summary>
/// Base exception for all task service exceptions
/// </summary>
public abstract class TaskServiceException : Exception
{
    public string Code { get; }
    public string Details { get; }
    public int StatusCode { get; }

    protected TaskServiceException(string code, string message, string details = "", int statusCode = 500, Exception? innerException = null)
        : base(message, innerException)
    {
        Code = code;
        Details = details;
        StatusCode = statusCode;
    }
}

/// <summary>
/// Exception for when a requested resource is not found
/// </summary>
public class NotFoundException : TaskServiceException
{
    public NotFoundException(string message, string details = "")
        : base("NOT_FOUND", message, details, 404)
    {
    }
}

/// <summary>
/// Exception for validation failures
/// </summary>
public class TaskValidationException : TaskServiceException
{
    public TaskValidationException(string message, string details = "")
        : base("VALIDATION_ERROR", message, details, 400)
    {
    }
}

/// <summary>
/// Exception for business rule violations
/// </summary>
public class BusinessRuleException : TaskServiceException
{
    public BusinessRuleException(string message, string details = "")
        : base("BUSINESS_RULE_VIOLATION", message, details, 400)
    {
    }
}

/// <summary>
/// Exception for external service failures
/// </summary>
public class ExternalServiceException : TaskServiceException
{
    public ExternalServiceException(string message, string details = "", Exception? innerException = null)
        : base("EXTERNAL_SERVICE_ERROR", message, details, 502, innerException)
    {
    }
}

/// <summary>
/// Exception for unauthorized access attempts
/// </summary>
public class UnauthorizedException : TaskServiceException
{
    public UnauthorizedException(string message, string details = "")
        : base("UNAUTHORIZED", message, details, 401)
    {
    }
}

/// <summary>
/// Exception for unexpected system errors
/// </summary>
public class SystemException : TaskServiceException
{
    public SystemException(string message, string details = "", Exception? innerException = null)
        : base("SYSTEM_ERROR", message, details, 500, innerException)
    {
    }
}