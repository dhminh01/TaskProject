using MediatR;
using TaskService.Domain.Entities;
using TaskService.Domain.Events;
using TaskService.Domain.Interfaces;

namespace TaskService.Application.Tasks.Commands.CreateTask;

public class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, CreateTaskResult>
{
    private readonly ITaskRepository _taskRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPublisher _publisher;

    public CreateTaskCommandHandler(
        ITaskRepository taskRepository,
        IUnitOfWork unitOfWork,
        IPublisher publisher)
    {
        _taskRepository = taskRepository;
        _unitOfWork = unitOfWork;
        _publisher = publisher;
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

        // Publish domain event
        await _publisher.Publish(taskCreatedEvent, cancellationToken);

        return new CreateTaskResult(
            taskItem.Id,
            taskItem.Title,
            taskItem.Description,
            taskItem.DueDate,
            taskItem.DateCreated);
    }
}