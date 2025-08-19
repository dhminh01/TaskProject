using MediatR;

namespace TaskService.Domain.Events;

public record TaskCreatedEvent : INotification
{
    public Guid TaskId { get; }
    public string Title { get; }
    public string Description { get; }
    public DateTime? DueDate { get; }

    public TaskCreatedEvent(Guid taskId, string title, string description, DateTime? dueDate)
    {
        TaskId = taskId;
        Title = title;
        Description = description;
        DueDate = dueDate;
    }
}