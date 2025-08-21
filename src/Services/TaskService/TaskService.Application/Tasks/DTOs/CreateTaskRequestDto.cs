namespace TaskService.Application.Tasks.DTOs;

public class CreateTaskRequestDto
{
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public DateTime? DueDate { get; set; }

    public CreateTaskRequestDto()
    {
    }

    public CreateTaskRequestDto(string title, string description, DateTime? dueDate = null)
    {
        Title = title;
        Description = description;
        DueDate = dueDate;
    }
}
