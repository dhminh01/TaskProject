using System;

namespace TaskService.Domain.Events;

public record TaskUpdatedEvent(
    Guid Id,
    string Title,
    string Description,
    DateTime? DueDate);
