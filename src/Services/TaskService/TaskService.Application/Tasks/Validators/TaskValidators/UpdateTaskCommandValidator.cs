using FluentValidation;
using TaskService.Application.Tasks.Commands;
using TaskService.Domain.Common.Exceptions;
using TaskService.Domain.Interfaces;

namespace TaskService.Application.Tasks.Validators;

public class UpdateTaskCommandValidator : AbstractValidator<UpdateTaskCommand>
{
    private readonly ITaskRepository _taskRepository;

    public UpdateTaskCommandValidator(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;

        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Task Id is required");

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Title is required")
            .MaximumLength(200)
            .WithMessage("Title must not exceed 200 characters")
            .MustAsync(async (title, cancellation) =>
            {
                var existingTask = await _taskRepository.GetByTitleAsync(title, cancellation);
                if (existingTask != null)
                    throw new DuplicateTaskTitleException(title);
                return true;
            })
            .WithMessage(x => $"A task with the title '{x.Title}' already exists");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Description is required")
            .MaximumLength(1000)
            .WithMessage("Description must not exceed 1000 characters");

        RuleFor(x => x.DueDate)
            .Must(dueDate => !dueDate.HasValue || dueDate.Value.ToUniversalTime() > DateTime.UtcNow)
            .WithMessage("Due date must be a future date and time")
            .When(x => x.DueDate.HasValue);
    }
}
