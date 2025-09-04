using MediatR;
using Microsoft.Extensions.Logging;
using TaskService.Domain.Common.Exceptions;
using TaskService.Domain.Entities;
using TaskService.Domain.Events;
using TaskService.Domain.Interfaces;
using TaskService.Application.Tasks.DTOs;
using MassTransit;

namespace TaskService.Application.Tasks.Commands.Handlers;

public class UpdateTaskCommandHandler : IRequestHandler<UpdateTaskCommand, UpdateTaskResponseDto>
{
    private readonly ITaskRepository _taskRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<UpdateTaskCommandHandler> _logger;

    public UpdateTaskCommandHandler(
        ITaskRepository taskRepository,
        IUnitOfWork unitOfWork,
        IPublishEndpoint publishEndpoint,
        ILogger<UpdateTaskCommandHandler> logger)
    {
        _taskRepository = taskRepository;
        _unitOfWork = unitOfWork;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task<UpdateTaskResponseDto> Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
    {
        // Validate due date
        if (request.DueDate.HasValue)
        {
            var utcDueDate = request.DueDate.Value.ToUniversalTime();
            if (utcDueDate <= DateTime.UtcNow)
            {
                throw new InvalidDueDateException();
            }
        }

        // Get the task
        var taskItem = await _taskRepository.GetByIdAsync(request.Id, cancellationToken);
        if (taskItem == null)
        {
            throw new TaskNotFoundException(request.Id);
        }

        // Check for duplicate title if title is being changed
        if (taskItem.Title != request.Title)
        {
            var existingTask = await _taskRepository.GetByTitleAsync(request.Title, cancellationToken);
            if (existingTask != null && existingTask.Id != request.Id)
            {
                throw new DuplicateTaskTitleException(request.Title);
            }
        }

        // Update the task
        taskItem.Update(request.Title, request.Description, request.DueDate);

        await _taskRepository.UpdateAsync(taskItem, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var taskUpdatedEvent = new TaskUpdatedEvent(
            taskItem.Id,
            taskItem.Title,
            taskItem.Description,
            taskItem.DueDate);

        try
        {
            // Publish event to RabbitMQ
            await _publishEndpoint.Publish(taskUpdatedEvent, cancellationToken);
            _logger.LogInformation("TaskUpdatedEvent published successfully for task {TaskId}", taskItem.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing TaskUpdatedEvent for task {TaskId}", taskItem.Id);
            throw;
        }

        return new UpdateTaskResponseDto(
            taskItem.Id,
            taskItem.Title,
            taskItem.Description,
            taskItem.DueDate,
            taskItem.DateCreated,
            taskItem.DateModified);
    }
}
