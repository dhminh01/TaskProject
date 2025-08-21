using MassTransit;
using TaskProject.Proto;

namespace TaskService.API.Consumers;

public class EmailConfirmationConsumer : IConsumer<EmailSentRequest>
{
    private readonly ILogger<EmailConfirmationConsumer> _logger;
    private readonly EmailConfirmationHandler _emailConfirmationHandler;

    public EmailConfirmationConsumer(
        ILogger<EmailConfirmationConsumer> logger,
        EmailConfirmationHandler emailConfirmationHandler)
    {
        _logger = logger;
        _emailConfirmationHandler = emailConfirmationHandler;
    }

    public async Task Consume(ConsumeContext<EmailSentRequest> context)
    {
        var notification = context.Message;
        using var scope = _logger.BeginScope("EmailSentNotification {TaskId}", notification.TaskId);

        try
        {
            _logger.LogInformation("Started processing email sent notification for task {TaskId}", notification.TaskId);

            await _emailConfirmationHandler.HandleEmailNotification(notification);

            _logger.LogInformation("Successfully processed email sent notification for task \"{TaskTitle}\" at {TimeStamp}", notification.TaskTitle, notification.SentTimestamp);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process email sent notification for task {TaskId}", notification.TaskId);
            throw;
        }
    }
}
