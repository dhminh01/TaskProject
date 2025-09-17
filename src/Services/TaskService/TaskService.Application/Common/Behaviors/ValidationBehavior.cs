using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using TaskService.Domain.Common.Exceptions;

namespace TaskService.Application.Common.Behaviors;

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    private readonly ILogger<ValidationBehavior<TRequest, TResponse>> _logger;

    public ValidationBehavior(
        IEnumerable<IValidator<TRequest>> validators,
        ILogger<ValidationBehavior<TRequest, TResponse>> logger)
    {
        _validators = validators;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);
        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .Where(r => r.Errors.Any())
            .SelectMany(r => r.Errors)
            .ToList();

        if (failures.Any())
        {
            _logger.LogWarning(
                "Validation failed for {RequestType}. Errors: {ValidationErrors}",
                typeof(TRequest).Name,
                string.Join(", ", failures.Select(f => f.ErrorMessage)));

            // Group validation errors by property
            var errorsByProperty = failures
                .GroupBy(f => f.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(f => f.ErrorMessage).ToList()
                );

            // Create a detailed error message
            var details = string.Join("\n", errorsByProperty.Select(kvp =>
                $"{kvp.Key}: {string.Join(", ", kvp.Value)}"));

            throw new TaskValidationException(
                $"Validation failed for {typeof(TRequest).Name}",
                details);
        }

        _logger.LogInformation(
            "Validation successful for {RequestType}",
            typeof(TRequest).Name);

        return await next();
    }
}
