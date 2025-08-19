using MediatR;
using Microsoft.Extensions.Logging;
using TaskService.Domain.Events;

namespace TaskService.Application.Tasks.EventHandlers;

public class TaskCreatedEventHandler : INotificationHandler<TaskCreatedEvent>
{
    private readonly ILogger<TaskCreatedEventHandler> _logger;

    public TaskCreatedEventHandler(ILogger<TaskCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(TaskCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("TaskService Domain Event: {DomainEvent}", notification.GetType().Name);

        return Task.CompletedTask;
    }
}