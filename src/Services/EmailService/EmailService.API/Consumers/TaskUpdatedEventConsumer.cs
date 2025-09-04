using EmailService.API.Services;
using MassTransit;
using TaskService.Domain.Events;
using TaskProject.Proto;

namespace EmailService.API.Consumers;

public class TaskUpdatedEventConsumer : IConsumer<TaskUpdatedEvent>
{
    private readonly IEmailService _emailService;
    private readonly ILogger<TaskUpdatedEventConsumer> _logger;
    private readonly IConfiguration _configuration;

    public TaskUpdatedEventConsumer(
        IEmailService emailService,
        ILogger<TaskUpdatedEventConsumer> logger,
        IConfiguration configuration)
    {
        _emailService = emailService;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task Consume(ConsumeContext<TaskUpdatedEvent> context)
    {
        var @event = context.Message;

        try
        {
            _logger.LogInformation("Processing TaskUpdatedEvent for task {TaskId}", @event.Id);

            // Prepare email content
            var subject = $"Task Updated: {@event.Title}";
            var body = $@"
                <h2>Task Updated</h2>
                <p>A task has been updated:</p>
                <ul>
                    <li><strong>Title:</strong> {@event.Title}</li>
                    <li><strong>Description:</strong> {@event.Description}</li>
                    <li><strong>Due Date:</strong> {@event.DueDate:dddd, MMMM dd, yyyy h:mm tt}</li>
                    <li><strong>Updated At:</strong> {DateTime.UtcNow:dddd, MMMM dd, yyyy h:mm tt}</li>
                </ul>
            ";

            // Send email notification
            var recipientEmail = _configuration["Gmail:RecipientEmail"]
                ?? throw new InvalidOperationException("Recipient email is not configured");
            await _emailService.SendEmailAsync(
                recipientEmail,
                subject,
                body);

            _logger.LogInformation("Email sent successfully for task {TaskId}", @event.Id);

            // Publish email sent notification
            await context.Publish(new EmailSentRequest
            {
                TaskId = @event.Id.ToString(),
                TaskTitle = @event.Title,
                EmailStatus = "Sent",
                SentTimestamp = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow)
            });

            // Publish domain event
            await context.Publish(new EmailSentEvent
            {
                TaskId = @event.Id,
                EmailStatus = "Sent",
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing TaskUpdatedEvent for task {TaskId}", @event.Id);
            throw;
        }
    }
}
