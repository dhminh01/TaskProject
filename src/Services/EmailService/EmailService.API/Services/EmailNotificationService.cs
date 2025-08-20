using Grpc.Core;
using TaskProject.EmailService;

namespace EmailService.API.Services;

public class EmailNotificationService : EmailNotification.EmailNotificationBase
{
    private readonly ILogger<EmailNotificationService> _logger;

    public EmailNotificationService(ILogger<EmailNotificationService> logger)
    {
        _logger = logger;
    }

    public override Task<EmailSentResponse> NotifyEmailSent(EmailSentRequest request, ServerCallContext context)
    {
        _logger.LogInformation("Email notification received for task {TaskId}. Status: {Status}, Timestamp: {Timestamp}",
            request.TaskId, request.EmailStatus, request.SentTimestamp);

        return Task.FromResult(new EmailSentResponse
        {
            Success = true,
            Message = $"Email notification for task {request.TaskId} processed successfully"
        });
    }
}
