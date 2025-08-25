using MediatR;
using TaskService.Application.Tasks.Commands;
using TaskService.Application.Tasks.DTOs;
using TaskService.Domain.Common.Exceptions;

namespace TaskService.Api.GraphQL.Mutations;

public class TaskMutations
{
    public async Task<CreateTaskPayload> CreateTaskAsync(
        CreateTaskInput input,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new CreateTaskCommand
            {
                Title = input.Title,
                Description = input.Description,
                DueDate = input.DueDate
            };

            var result = await mediator.Send(command, cancellationToken);
            return new CreateTaskPayload(result);
        }
        catch (TaskValidationException ex)
        {
            throw new GraphQLException(new Error(ex.Message, "VALIDATION_ERROR"));
        }
        catch (Exception)
        {
            throw new GraphQLException(new Error("An unexpected error occurred", "INTERNAL_ERROR"));
        }
    }

    [GraphQLDescription("Delete a task")]
    public async Task<DeleteTaskPayload> DeleteTaskAsync(
        DeleteTaskInput input,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new DeleteTaskCommand(input.Id);
        var result = await mediator.Send(command, cancellationToken);
        return new DeleteTaskPayload(result);
    }
}

public record CreateTaskInput(
    string Title,
    string Description,
    DateTime? DueDate = null);

public record CreateTaskPayload(CreateTaskResponseDto Task);

public record DeleteTaskInput(Guid Id);

public record DeleteTaskPayload(bool Success);

public class CreateTaskInputType : InputObjectType<CreateTaskInput>
{
    protected override void Configure(IInputObjectTypeDescriptor<CreateTaskInput> descriptor)
    {
        descriptor.Field(f => f.Title)
            .Type<NonNullType<StringType>>();

        descriptor.Field(f => f.Description)
            .Type<NonNullType<StringType>>();

        descriptor.Field(f => f.DueDate)
            .Type<DateTimeType>();
    }
}

public class CreateTaskPayloadType : ObjectType<CreateTaskPayload>
{
    protected override void Configure(IObjectTypeDescriptor<CreateTaskPayload> descriptor)
    {
        descriptor.Field(f => f.Task)
            .Type<CreateTaskResultType>();
    }
}

public class CreateTaskResultType : ObjectType<CreateTaskResponseDto>
{
    protected override void Configure(IObjectTypeDescriptor<CreateTaskResponseDto> descriptor)
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

public class DeleteTaskInputType : InputObjectType<DeleteTaskInput>
{
    protected override void Configure(IInputObjectTypeDescriptor<DeleteTaskInput> descriptor)
    {
        descriptor.Field(f => f.Id)
            .Type<NonNullType<UuidType>>();
    }
}

public class DeleteTaskPayloadType : ObjectType<DeleteTaskPayload>
{
    protected override void Configure(IObjectTypeDescriptor<DeleteTaskPayload> descriptor)
    {
        descriptor.Field(f => f.Success)
            .Type<NonNullType<BooleanType>>();
    }
}