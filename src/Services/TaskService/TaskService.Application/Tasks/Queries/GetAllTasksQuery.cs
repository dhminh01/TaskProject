using MediatR;
using TaskService.Domain.Entities;

namespace TaskService.Application.Tasks.Queries;

public record GetAllTasksQuery : IRequest<IEnumerable<TaskItem>>;

