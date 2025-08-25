using MediatR;
using TaskService.Application.Tasks.DTOs;

namespace TaskService.Application.Tasks.Queries;

public record GetAllTasksQuery : IRequest<IEnumerable<GetAllTasksRequestDto>>;
