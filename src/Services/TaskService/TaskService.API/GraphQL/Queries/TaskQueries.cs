using MediatR;
using TaskService.Application.Tasks.Queries;
using TaskService.Domain.Entities;

namespace TaskService.Api.GraphQL.Queries;

[GraphQLDescription("Queries for tasks")]
public class TaskQueries
{
    [GraphQLDescription("Get a single task by ID")]
    public async Task<GetTaskDetailResult?> GetTaskAsync(
        [GraphQLDescription("The unique identifier of the task")] Guid id,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        return await mediator.Send(new GetTaskDetailQuery(id), cancellationToken);
    }

    [GraphQLDescription("Get all tasks")]
    public async Task<IEnumerable<TaskItem>> GetTasksAsync(
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        return await mediator.Send(new GetAllTasksQuery(), cancellationToken);
    }
}