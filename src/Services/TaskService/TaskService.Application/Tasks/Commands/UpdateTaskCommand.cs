using MediatR;
using TaskService.Application.Tasks.DTOs;

namespace TaskService.Application.Tasks.Commands;

public class UpdateTaskCommand : IRequest<UpdateTaskResponseDto>
{
    public required Guid Id { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public DateTime? DueDate { get; set; }

    public UpdateTaskCommand()
    {
    }

    public UpdateTaskCommand(Guid id, UpdateTaskRequestDto dto)
    {
        Id = id;
        Title = dto.Title;
        Description = dto.Description;
        DueDate = dto.DueDate;
    }
}
