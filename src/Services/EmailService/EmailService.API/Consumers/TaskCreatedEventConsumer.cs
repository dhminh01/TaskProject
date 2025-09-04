using EmailService.API.Services;
using MassTransit;
using TaskService.Domain.Events;
using TaskProject.Proto;

namespace EmailService.API.Consumers;

public class TaskCreatedEventConsumer : IConsumer<TaskCreatedEvent>
{
    private readonly IEmailService _emailService;
    private readonly ILogger<TaskCreatedEventConsumer> _logger;
    private readonly IConfiguration _configuration;

    public TaskCreatedEventConsumer(
        IEmailService emailService,
        ILogger<TaskCreatedEventConsumer> logger,
        IConfiguration configuration)
    {
        _emailService = emailService;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task Consume(ConsumeContext<TaskCreatedEvent> context)
    {
        var @event = context.Message;

        try
        {
            _logger.LogInformation("Processing TaskCreatedEvent for task {TaskId}", @event.TaskId);

            // Prepare email content
            var subject = $"New Task Created: {@event.Title}";
            var body = $@"
                <h2>New Task Created</h2>
                <p>A new task has been created:</p>
                <ul>
                    <li><strong>Title:</strong> {@event.Title}</li>
                    <li><strong>Description:</strong> {@event.Description}</li>
                    <li><strong>Due Date:</strong> {@event.DueDate:dddd, MMMM dd, yyyy h:mm tt}</li>
                    <li><strong>Created At:</strong> {DateTime.UtcNow:dddd, MMMM dd, yyyy h:mm tt}</li>
                </ul>
            ";

            // Send email notification
            var recipientEmail = _configuration["Gmail:RecipientEmail"]
                ?? throw new InvalidOperationException("Recipient email is not configured");
            await _emailService.SendEmailAsync(
                recipientEmail,
                subject,
                body);

            _logger.LogInformation("Email sent successfully for task {TaskId}", @event.TaskId);

            // Publish email sent notification
            await context.Publish(new EmailSentRequest
            {
                TaskId = @event.TaskId.ToString(),
                TaskTitle = @event.Title,
                EmailStatus = "Sent",
                SentTimestamp = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow)
            });

            // Publish email sent notification
            await context.Publish(new EmailSentEvent
            {
                TaskId = @event.TaskId,
                EmailStatus = "Sent",
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing TaskCreatedEvent for task {TaskId}", @event.TaskId);
            throw;
        }
    }
}
