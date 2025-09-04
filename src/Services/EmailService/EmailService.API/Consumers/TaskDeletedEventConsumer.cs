using MassTransit;
using Microsoft.Extensions.Logging;
using TaskService.Domain.Events;
using EmailService.API.Services;
using Microsoft.Extensions.Configuration;

namespace EmailService.API.Consumers;

public class TaskDeletedEventConsumer : IConsumer<TaskDeletedEvent>
{
    private readonly IEmailService _emailService;
    private readonly ILogger<TaskDeletedEventConsumer> _logger;
    private readonly IConfiguration _configuration;

    public TaskDeletedEventConsumer(
        IEmailService emailService,
        ILogger<TaskDeletedEventConsumer> logger,
        IConfiguration configuration)
    {
        _emailService = emailService;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task Consume(ConsumeContext<TaskDeletedEvent> context)
    {
        var taskDeletedEvent = context.Message;

        try
        {
            var subject = $"Task Deleted: {taskDeletedEvent.Title}";
            var body = $@"
                <h2>Task has been deleted</h2>
                <p>Task details:</p>
                <ul>
                    <li><strong>Title:</strong> {taskDeletedEvent.Title}</li>
                    <li><strong>Description:</strong> {taskDeletedEvent.Description}</li>
                    <li><strong>Due Date:</strong> {(taskDeletedEvent.DueDate.HasValue ? taskDeletedEvent.DueDate.Value.ToString("dddd, MMMM dd, yyyy h:mm tt") : "Not set")}</li>
                    <li><strong>Deleted At:</strong> {taskDeletedEvent.DeletedAt.ToString("dddd, MMMM dd, yyyy h:mm tt")}</li>
                </ul>
            ";

            // Get recipient email from configuration
            var recipientEmail = _configuration["Gmail:RecipientEmail"]
                ?? throw new InvalidOperationException("Recipient email is not configured");
            await _emailService.SendEmailAsync(recipientEmail, subject, body);
            _logger.LogInformation("Task deletion email sent successfully for task {TaskId}", taskDeletedEvent.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending task deletion email for task {TaskId}", taskDeletedEvent.Id);
            throw;
        }
    }
}
