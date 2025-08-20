using Microsoft.Extensions.Logging;
using TaskProject.EmailService;

namespace TaskService.API.Services;

public class EmailNotificationHandler
{
    private readonly ILogger<EmailNotificationHandler> _logger;

    public EmailNotificationHandler(ILogger<EmailNotificationHandler> logger)
    {
        _logger = logger;
    }

    public async Task HandleEmailNotification(EmailSentRequest notification)
    {
        using var scope = _logger.BeginScope("EmailNotification {TaskId}", notification.TaskId);

        try
        {
            _logger.LogDebug("Starting to process email notification for task {TaskId}", notification.TaskId);

            _logger.LogInformation(
                "✉️ Email notification received - Task {TaskId} email status: {Status}",
                notification.TaskId,
                notification.EmailStatus);

            await Task.CompletedTask; // Ensure async/await is respected

            _logger.LogDebug("Successfully processed email notification for task {TaskId}", notification.TaskId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process email notification for task {TaskId}", notification.TaskId);
            throw;
        }
    }
}
