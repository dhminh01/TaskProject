namespace EmailService.Application.Interfaces;

public interface ITaskServiceClient
{
    Task<bool> AcknowledgeTaskEmailSentAsync(Guid taskId, bool success, string? errorMessage = null, CancellationToken cancellationToken = default);
}
