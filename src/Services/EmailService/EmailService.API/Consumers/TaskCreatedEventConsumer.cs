using EmailService.API.Services;
using MassTransit;
using TaskService.Domain.Events;
using TaskProject.EmailService;

namespace EmailService.API.Consumers;

public class TaskCreatedEventConsumer : IConsumer<TaskCreatedEvent>
{
    private readonly IEmailService _emailService;
    private readonly ILogger<TaskCreatedEventConsumer> _logger;

    public TaskCreatedEventConsumer(
        IEmailService emailService,
        ILogger<TaskCreatedEventConsumer> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<TaskCreatedEvent> context)
    {
        var @event = context.Message;

        try
        {
            _logger.LogInformation("Processing TaskCreatedEvent for task {TaskId}", @event.TaskId);

            // Prepare email content
            var subject = $"New Task Created: {@event.Title}";
            var body = $@"A new task has been created:
                        
Title: {@event.Title}
Description: {@event.Description}
Due Date: {@event.DueDate:yyyy-MM-dd}";

            // Send email notification
            await _emailService.SendEmailAsync(
                "dhminh.work@gmail.com",  // Replace with actual recipient
                subject,
                body);

            _logger.LogInformation("Email sent successfully for task {TaskId}", @event.TaskId);

            // Publish email sent notification
            await context.Publish(new EmailSentRequest
            {
                TaskId = @event.TaskId.ToString(),
                EmailStatus = "Sent",
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
