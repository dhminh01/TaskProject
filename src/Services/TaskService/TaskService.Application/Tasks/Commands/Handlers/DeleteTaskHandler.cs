using MediatR;
using TaskService.Domain.Interfaces;

namespace TaskService.Application.Tasks.Commands;

public class DeleteTaskHandler : IRequestHandler<DeleteTaskCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITaskRepository _taskRepository;

    public DeleteTaskHandler(IUnitOfWork unitOfWork, ITaskRepository taskRepository)
    {
        _unitOfWork = unitOfWork;
        _taskRepository = taskRepository;
    }

    public async Task<bool> Handle(DeleteTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await _taskRepository.GetByIdAsync(request.Id, cancellationToken);
        if (task == null)
        {
            return false;
        }

        await _taskRepository.DeleteAsync(task, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}