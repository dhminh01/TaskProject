using MediatR;
using MassTransit;
using Microsoft.Extensions.Logging;
using TaskService.Domain.Entities;
using TaskService.Domain.Events;
using TaskService.Domain.Interfaces;

namespace TaskService.Application.Tasks.Commands.CreateTask;

public class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, CreateTaskResult>
{
    private readonly ITaskRepository _taskRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<CreateTaskCommandHandler> _logger;

    public CreateTaskCommandHandler(
        ITaskRepository taskRepository,
        IUnitOfWork unitOfWork,
        IPublishEndpoint publishEndpoint,
        ILogger<CreateTaskCommandHandler> logger)
    {
        _taskRepository = taskRepository;
        _unitOfWork = unitOfWork;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task<CreateTaskResult> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        var taskItem = new TaskItem(
            request.Title,
            request.Description,
            request.DueDate);

        // Add domain event
        var taskCreatedEvent = new TaskCreatedEvent(
            taskItem.Id,
            taskItem.Title,
            taskItem.Description,
            taskItem.DueDate);

        await _taskRepository.AddAsync(taskItem, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        try
        {
            // Publish event to RabbitMQ
            await _publishEndpoint.Publish(taskCreatedEvent, cancellationToken);
            _logger.LogInformation("TaskCreatedEvent published successfully for task {TaskId}", taskItem.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing TaskCreatedEvent for task {TaskId}", taskItem.Id);
            throw;
        }

        return new CreateTaskResult(
            taskItem.Id,
            taskItem.Title,
            taskItem.Description,
            taskItem.DueDate,
            taskItem.DateCreated);
    }
}