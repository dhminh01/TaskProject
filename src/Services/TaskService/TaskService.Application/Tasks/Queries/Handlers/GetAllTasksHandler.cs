using MediatR;
using TaskService.Application.Tasks.DTOs;
using TaskService.Domain.Interfaces;

namespace TaskService.Application.Tasks.Queries;

public class GetAllTasksHandler : IRequestHandler<GetAllTasksQuery, IEnumerable<GetAllTasksRequestDto>>
{
    private readonly ITaskRepository _taskRepository;

    public GetAllTasksHandler(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async Task<IEnumerable<GetAllTasksRequestDto>> Handle(GetAllTasksQuery request, CancellationToken cancellationToken)
    {
        var tasks = await _taskRepository.GetAllAsync(cancellationToken);
        return tasks.Select(task => new GetAllTasksRequestDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            DueDate = task.DueDate,
            DateCreated = task.DateCreated
        });
    }
}