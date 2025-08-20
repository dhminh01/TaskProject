using MassTransit;
using Microsoft.Extensions.Logging;
using TaskProject.EmailService;
using TaskService.API.Services;

namespace TaskService.API.Consumers;

public class EmailSentNotificationConsumer : IConsumer<EmailSentRequest>
{
    private readonly ILogger<EmailSentNotificationConsumer> _logger;
    private readonly EmailNotificationHandler _emailNotificationHandler;

    public EmailSentNotificationConsumer(
        ILogger<EmailSentNotificationConsumer> logger,
        EmailNotificationHandler emailNotificationHandler)
    {
        _logger = logger;
        _emailNotificationHandler = emailNotificationHandler;
    }

    public async Task Consume(ConsumeContext<EmailSentRequest> context)
    {
        var notification = context.Message;
        using var scope = _logger.BeginScope("EmailSentNotification {TaskId}", notification.TaskId);

        try
        {
            _logger.LogInformation("Started processing email sent notification for task {TaskId}", notification.TaskId);

            await _emailNotificationHandler.HandleEmailNotification(notification);

            _logger.LogInformation("Successfully processed email sent notification for task {TaskId}", notification.TaskId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process email sent notification for task {TaskId}", notification.TaskId);
            throw;
        }
    }
}
