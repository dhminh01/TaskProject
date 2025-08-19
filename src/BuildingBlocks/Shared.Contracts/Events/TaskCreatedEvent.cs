namespace Shared.Contracts.Events;

/// <summary>
/// Event published when a task is created - consumed by EmailService
/// </summary>
public class TaskCreatedEvent
{
    public Guid TaskId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public DateTime DateCreated { get; set; }
    public string? RecipientEmail { get; set; }
    public DateTime EventTimestamp { get; set; } = DateTime.UtcNow;
}