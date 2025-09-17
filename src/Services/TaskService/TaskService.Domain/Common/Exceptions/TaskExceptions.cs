namespace TaskService.Domain.Common.Exceptions;

/// <summary>
/// Exception thrown when a task is not found
/// </summary>
public class TaskNotFoundException : NotFoundException
{
    public TaskNotFoundException(Guid taskId)
        : base($"Task with ID {taskId} was not found", $"TaskId: {taskId}")
    {
    }
}

/// <summary>
/// Exception thrown when attempting to create a task with a duplicate title
/// </summary>
public class DuplicateTaskTitleException : TaskValidationException
{
    public DuplicateTaskTitleException(string title)
        : base($"A task with the title '{title}' already exists", $"Title: {title}")
    {
    }
}

/// <summary>
/// Exception thrown when a task's due date is invalid
/// </summary>
public class InvalidDueDateException : TaskValidationException
{
    public InvalidDueDateException(DateTime? dueDate)
        : base("Due date must be in the future", $"DueDate: {dueDate}")
    {
    }
}

/// <summary>
/// Exception thrown when task publishing fails
/// </summary>
public class TaskPublishException : ExternalServiceException
{
    public TaskPublishException(Guid taskId, Exception innerException)
        : base(
            $"Failed to publish task event for task {taskId}",
            $"TaskId: {taskId}",
            innerException)
    {
    }
}