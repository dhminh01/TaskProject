using MediatR;
using TaskService.Domain.Interfaces;
using MassTransit;
using TaskService.Domain.Events;

namespace TaskService.Application.Tasks.Commands;

public class DeleteTaskHandler : IRequestHandler<DeleteTaskCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITaskRepository _taskRepository;
    private readonly IPublishEndpoint _publishEndpoint;

    public DeleteTaskHandler(
        IUnitOfWork unitOfWork,
        ITaskRepository taskRepository,
        IPublishEndpoint publishEndpoint)
    {
        _unitOfWork = unitOfWork;
        _taskRepository = taskRepository;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<bool> Handle(DeleteTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await _taskRepository.GetByIdAsync(request.Id, cancellationToken);
        if (task == null)
        {
            return false;
        }

        await _taskRepository.DeleteAsync(task, cancellationToken);
        var result = await _unitOfWork.SaveChangesAsync(cancellationToken) > 0;

        if (result)
        {
            // Publish task deleted event
            await _publishEndpoint.Publish(new TaskDeletedEvent(
                task.Id,
                task.Title,
                task.Description,
                task.DueDate,
                DateTime.UtcNow
            ), cancellationToken);
        }

        return result;
    }
}