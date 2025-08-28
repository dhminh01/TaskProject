namespace TaskService.Application.Tasks.DTOs;

public record UpdateTaskRequestDto(
    string Title,
    string Description,
    DateTime? DueDate = null);

public record UpdateTaskResponseDto(
    Guid Id,
    string Title,
    string Description,
    DateTime? DueDate,
    DateTime DateCreated,
    DateTime? DateModified);
