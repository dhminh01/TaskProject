using MediatR;
using TaskService.Application.Tasks.DTOs;
using TaskService.Application.Tasks.Queries;

namespace TaskService.Api.GraphQL.Queries;

public class TaskType : ObjectType<GetAllTasksRequestDto>
{
    protected override void Configure(IObjectTypeDescriptor<GetAllTasksRequestDto> descriptor)
    {
        descriptor.Field(f => f.Id)
            .Type<NonNullType<IdType>>();

        descriptor.Field(f => f.Title)
            .Type<NonNullType<StringType>>();

        descriptor.Field(f => f.Description)
            .Type<NonNullType<StringType>>();

        descriptor.Field(f => f.DueDate)
            .Type<DateTimeType>();

        descriptor.Field(f => f.DateCreated)
            .Type<NonNullType<DateTimeType>>();
    }
}

public class TaskDetailType : ObjectType<GetTaskDetailDto>
{
    protected override void Configure(IObjectTypeDescriptor<GetTaskDetailDto> descriptor)
    {
        descriptor.Field(f => f.Id)
            .Type<NonNullType<IdType>>();

        descriptor.Field(f => f.Title)
            .Type<NonNullType<StringType>>();

        descriptor.Field(f => f.Description)
            .Type<NonNullType<StringType>>();

        descriptor.Field(f => f.DueDate)
            .Type<DateTimeType>();

        descriptor.Field(f => f.DateCreated)
            .Type<NonNullType<DateTimeType>>();
    }
}

[GraphQLDescription("Queries for tasks")]
public class TaskQueries
{
    [GraphQLDescription("Get a single task by ID")]
    public async Task<GetTaskDetailDto?> GetTaskAsync(
        [GraphQLDescription("The unique identifier of the task")] Guid id,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        return await mediator.Send(new GetTaskDetailQuery(id), cancellationToken);
    }

    [GraphQLDescription("Get all tasks")]
    public async Task<IEnumerable<GetAllTasksRequestDto>> GetTasksAsync(
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        return await mediator.Send(new GetAllTasksQuery(), cancellationToken);
    }
}