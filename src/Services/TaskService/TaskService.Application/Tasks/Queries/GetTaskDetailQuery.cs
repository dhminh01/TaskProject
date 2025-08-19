
using MediatR;

namespace TaskService.Application.Tasks.Queries;

public record GetTaskDetailQuery(Guid Id) : IRequest<GetTaskDetailResult>;

public record GetTaskDetailResult(
    Guid Id,
    string Title,
    string Description,
    DateTime? DueDate,
    DateTime DateCreated
);
