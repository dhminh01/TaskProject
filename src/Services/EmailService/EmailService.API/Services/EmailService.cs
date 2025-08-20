
namespace EmailService.API.Services;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        // TODO: Implement actual email sending logic here (e.g., using SMTP, SendGrid, etc.)
        _logger.LogInformation("Sending email to: {To}, Subject: {Subject}", to, subject);

        // Simulate email sending delay
        await Task.Delay(100);

        _logger.LogInformation("Email sent successfully to: {To}", to);
    }
}
