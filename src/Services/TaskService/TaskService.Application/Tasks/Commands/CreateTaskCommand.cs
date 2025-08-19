using MediatR;

namespace TaskService.Application.Tasks.Commands;

public record CreateTaskCommand(
    string Title,
    string Description,
    DateTime? DueDate = null
) : IRequest<CreateTaskResult>;

public record CreateTaskResult(
    Guid Id,
    string Title,
    string Description,
    DateTime? DueDate,
    DateTime DateCreated
);