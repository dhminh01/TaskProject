namespace TaskService.Domain.Common.Exceptions;

public class TaskValidationException : TaskServiceException
{
    public TaskValidationException(string message) : base("VALIDATION_ERROR", message)
    {
    }
}

public class DuplicateTaskTitleException : TaskValidationException
{
    public DuplicateTaskTitleException(string title)
        : base($"A task with the title '{title}' already exists")
    {
    }
}

public class InvalidDueDateException : TaskValidationException
{
    public InvalidDueDateException()
        : base("Due date must be in the future")
    {
    }
}
