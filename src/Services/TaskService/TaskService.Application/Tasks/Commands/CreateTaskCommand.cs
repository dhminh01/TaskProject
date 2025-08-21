using MediatR;
using TaskService.Application.Tasks.DTOs;

namespace TaskService.Application.Tasks.Commands;

public class CreateTaskCommand : IRequest<CreateTaskResponseDto>
{
    public required string Title { get; set; }
    public required string Description { get; set; }
    public DateTime? DueDate { get; set; }

    public CreateTaskCommand()
    {
    }

    public CreateTaskCommand(CreateTaskRequestDto dto)
    {
        Title = dto.Title;
        Description = dto.Description;
        DueDate = dto.DueDate;
    }
}