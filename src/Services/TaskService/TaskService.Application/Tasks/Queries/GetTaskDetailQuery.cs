
using MediatR;
using TaskService.Application.Tasks.DTOs;

namespace TaskService.Application.Tasks.Queries;

public class GetTaskDetailQuery(Guid id) : IRequest<GetTaskDetailDto>
{
    public Guid Id { get; } = id;
}
