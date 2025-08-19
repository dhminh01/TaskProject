namespace Shared.Contracts.Events;

public class TaskReminderEvent
{
    public Guid TaskId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public string? RecipientEmail { get; set; }
    public DateTime EventTimestamp { get; set; } = DateTime.UtcNow;
    public int DaysUntilDue { get; set; }
}
