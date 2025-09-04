namespace TaskService.Domain.Events;

public record TaskDeletedEvent(
    Guid Id,
    string Title,
    string Description,
    DateTime? DueDate,
    DateTime DeletedAt
);
