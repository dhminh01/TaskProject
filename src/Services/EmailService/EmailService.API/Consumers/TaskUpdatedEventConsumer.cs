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
            var subject = $"Task Updated: {@event.NewTitle}";

            var changes = new List<string>();
            if (@event.OldTitle != @event.NewTitle)
                changes.Add($"<li><strong>Title:</strong> Changed from \"{@event.OldTitle}\" to \"{@event.NewTitle}\"</li>");

            if (@event.OldDescription != @event.NewDescription)
                changes.Add($"<li><strong>Description:</strong> Changed from \"{@event.OldDescription}\" to \"{@event.NewDescription}\"</li>");

            if (@event.OldDueDate != @event.NewDueDate)
            {
                var oldDueDate = @event.OldDueDate?.ToString("dddd, MMMM dd, yyyy h:mm tt") ?? "Not set";
                var newDueDate = @event.NewDueDate?.ToString("dddd, MMMM dd, yyyy h:mm tt") ?? "Not set";
                changes.Add($"<li><strong>Due Date:</strong> Changed from {oldDueDate} to {newDueDate}</li>");
            }

            var changesHtml = string.Join("\n", changes);
            var body = $@"
                <h2>Task Updated</h2>
                <p>The following changes were made to the task:</p>
                <ul>
                    {changesHtml}
                </ul>
                <p><strong>Updated At:</strong> {@event.UpdatedAt:dddd, MMMM dd, yyyy h:mm tt}</p>
                
                <h3>Current Task Details:</h3>
                <ul>
                    <li><strong>Title:</strong> {@event.NewTitle}</li>
                    <li><strong>Description:</strong> {@event.NewDescription}</li>
                    <li><strong>Due Date:</strong> {@event.NewDueDate?.ToString("dddd, MMMM dd, yyyy h:mm tt") ?? "Not set"}</li>
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
                TaskTitle = @event.NewTitle,
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
