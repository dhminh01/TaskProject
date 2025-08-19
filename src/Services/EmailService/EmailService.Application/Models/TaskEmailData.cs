namespace EmailService.Application.Models;

public class TaskEmailData
{
    public Guid TaskId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public DateTime DateCreated { get; set; }
    public string RecipientEmail { get; set; } = string.Empty;
}