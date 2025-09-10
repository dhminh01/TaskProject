using FluentValidation;
using TaskService.Application.Tasks.Commands;
using TaskService.Domain.Common.Exceptions;
using TaskService.Domain.Interfaces;

namespace TaskService.Application.Tasks.Validators;

public class CreateTaskCommandValidator : AbstractValidator<CreateTaskCommand>
{
    private readonly ITaskRepository _taskRepository;

    public CreateTaskCommandValidator(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;

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
            .WithMessage("Title must be unique");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Description is required")
            .MaximumLength(1000)
            .WithMessage("Description must not exceed 1000 characters");

        RuleFor(x => x.DueDate)
            .Must(dueDate =>
            {
                if (dueDate.HasValue && dueDate.Value <= DateTime.UtcNow)
                    throw new InvalidDueDateException();
                return true;
            })
            .When(x => x.DueDate.HasValue);
    }
}