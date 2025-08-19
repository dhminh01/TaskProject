using MediatR;
using TaskService.Domain.Entities;
using TaskService.Domain.Interfaces;

namespace TaskService.Application.Tasks.Queries;

public class GetAllTasksHandler : IRequestHandler<GetAllTasksQuery, IEnumerable<TaskItem>>
{
    private readonly ITaskRepository _taskRepository;

    public GetAllTasksHandler(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async Task<IEnumerable<TaskItem>> Handle(GetAllTasksQuery request, CancellationToken cancellationToken)
    {
        return await _taskRepository.GetAllAsync(cancellationToken);
    }
}