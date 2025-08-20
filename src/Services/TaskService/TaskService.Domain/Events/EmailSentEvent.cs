namespace TaskService.Domain.Events;

public class EmailSentEvent
{
    public Guid TaskId { get; set; }
    public string EmailStatus { get; set; }
}
