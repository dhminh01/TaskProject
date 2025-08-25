namespace TaskService.Domain.Common.Exceptions;

public class TaskServiceException : Exception
{
    public string? Code { get; }

    public TaskServiceException(string message) : base(message)
    {
    }

    public TaskServiceException(string code, string message) : base(message)
    {
        Code = code;
    }

    public TaskServiceException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public TaskServiceException(string code, string message, Exception innerException) : base(message, innerException)
    {
        Code = code;
    }
}
