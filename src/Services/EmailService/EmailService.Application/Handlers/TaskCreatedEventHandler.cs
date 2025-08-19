using MassTransit;
using EmailService.Application.Interfaces;
using EmailService.Application.Models;
using Shared.Contracts.Events;
using Microsoft.Extensions.Logging;
using EmailService.Application.Enums;

namespace EmailService.Application.Handlers;

public class TaskCreatedEventHandler : IConsumer<TaskCreatedEvent>
{
    private readonly IEmailService _emailService;
    private readonly ITaskServiceClient _taskServiceClient;
    private readonly ILogger<TaskCreatedEventHandler> _logger;

    public TaskCreatedEventHandler(
        IEmailService emailService,
        ITaskServiceClient taskServiceClient,
        ILogger<TaskCreatedEventHandler> logger)
    {
        _emailService = emailService;
        _taskServiceClient = taskServiceClient;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<TaskCreatedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Processing TaskCreated event for Task {TaskId}", message.TaskId);

        try
        {
            var taskData = new TaskEmailData
            {
                TaskId = message.TaskId,
                Title = message.Title,
                Description = message.Description,
                DueDate = message.DueDate,
                DateCreated = message.DateCreated,
                RecipientEmail = message.RecipientEmail ?? "default@example.com" // Should come from configuration
            };

            var result = await _emailService.SendTaskNotificationAsync(
                taskData,
                EmailTemplate.TaskCreated,
                context.CancellationToken);

            // Acknowledge back to TaskService via gRPC
            await _taskServiceClient.AcknowledgeTaskEmailSentAsync(
                message.TaskId,
                result.IsSuccess,
                result.ErrorMessage,
                context.CancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully sent task creation email for Task {TaskId}", message.TaskId);
            }
            else
            {
                _logger.LogError("Failed to send task creation email for Task {TaskId}: {Error}",
                    message.TaskId, result.ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing TaskCreated event for Task {TaskId}", message.TaskId);

            // Still acknowledge the failure to TaskService
            await _taskServiceClient.AcknowledgeTaskEmailSentAsync(
                message.TaskId,
                false,
                ex.Message,
                context.CancellationToken);

            throw; // Let MassTransit handle retry logic
        }
    }
}