namespace EmailService.Application.Models;

public class EmailMessage
{
    public string To { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? FromName { get; set; }
    public bool IsHtml { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
