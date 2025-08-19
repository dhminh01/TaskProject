namespace Shared.Contracts.DTOs;

public class TaskDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public DateTime DateCreated { get; set; }
    public DateTime DateUpdated { get; set; }
}