using TaskProject.Proto;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;

namespace TaskService.API.Consumers;

public class EmailConfirmationHandler
{
    private readonly EmailNotification.EmailNotificationClient _emailNotificationClient;
    private readonly ILogger<EmailConfirmationHandler> _logger;

    public EmailConfirmationHandler(
        EmailNotification.EmailNotificationClient emailNotificationClient,
        ILogger<EmailConfirmationHandler> logger)
    {
        _emailNotificationClient = emailNotificationClient;
        _logger = logger;
    }

    public async Task HandleEmailNotification(string taskId, string taskTitle, string emailStatus)
    {
        using var scope = _logger.BeginScope("EmailNotification {TaskId}", taskId);

        try
        {
            _logger.LogDebug("Starting to process email notification for task {TaskId}", taskId);

            var request = new EmailSentRequest
            {
                TaskId = taskId,
                TaskTitle = taskTitle,
                EmailStatus = emailStatus,
                SentTimestamp = Timestamp.FromDateTime(DateTime.UtcNow)
            };

            var response = await _emailNotificationClient.NotifyEmailSentAsync(request);

            if (response.Success)
            {
                _logger.LogInformation(
                    "Email notification confirmed - Task {TaskId} email status: {Status}",
                    taskId,
                    emailStatus);
            }
            else
            {
                _logger.LogWarning(
                    "Email notification failed - Task {TaskId} email status: {Status}",
                    taskId,
                    emailStatus);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process email notification for task {TaskId}", taskId);
            throw;
        }
    }
}
