using MediatR;
using TaskService.Domain.Interfaces;

namespace TaskService.Application.Tasks.Queries.GetTaskDetail;

public class GetTaskDetailHandler : IRequestHandler<GetTaskDetailQuery, GetTaskDetailResult?>
{
    private readonly ITaskRepository _taskRepository;

    public GetTaskDetailHandler(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async Task<GetTaskDetailResult?> Handle(GetTaskDetailQuery request, CancellationToken cancellationToken)
    {
        var taskItem = await _taskRepository.GetByIdAsync(request.Id, cancellationToken);

        if (taskItem is null)
            return null;

        return new GetTaskDetailResult(
            taskItem.Id,
            taskItem.Title,
            taskItem.Description,
            taskItem.DueDate,
            taskItem.DateCreated
        );
    }
}
