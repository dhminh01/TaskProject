namespace TaskService.Application.Tasks.DTOs;

public class CreateTaskResponseDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public DateTime? DueDate { get; set; }
    public DateTime DateCreated { get; set; }

    public CreateTaskResponseDto()
    {
    }

    public CreateTaskResponseDto(Guid id, string title, string description, DateTime? dueDate, DateTime dateCreated)
    {
        Id = id;
        Title = title;
        Description = description;
        DueDate = dueDate;
        DateCreated = dateCreated;
    }
}
