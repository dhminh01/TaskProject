namespace Shared.Contracts.DTOs;

public class EmailStatusDto
{
    public Guid TaskId { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime SentAt { get; set; }
    public string EmailType { get; set; } = string.Empty;
}