namespace TaskService.Domain.Common.Exceptions;

public class TaskNotFoundException : Exception
{
    public TaskNotFoundException(Guid taskId)
        : base($"Task with ID {taskId} was not found.")
    {
    }
}
