namespace TaskService.Domain.Events;

public record TaskUpdatedEvent(
    Guid Id,
    string OldTitle,
    string NewTitle,
    string OldDescription,
    string NewDescription,
    DateTime? OldDueDate,
    DateTime? NewDueDate,
    DateTime UpdatedAt);
