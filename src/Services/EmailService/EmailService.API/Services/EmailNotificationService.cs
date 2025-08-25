using Grpc.Core;
using TaskProject.Proto;
using Google.Protobuf.WellKnownTypes;

namespace EmailService.API.Services;

public class EmailNotificationService : EmailNotification.EmailNotificationBase
{
    private readonly ILogger<EmailNotificationService> _logger;

    public EmailNotificationService(ILogger<EmailNotificationService> logger)
    {
        _logger = logger;
    }

    public override async Task<EmailSentResponse> NotifyEmailSent(EmailSentRequest request, ServerCallContext context)
    {
        _logger.LogInformation(
            "Email notification received for task {TaskId}: {TaskTitle}, Status: {Status}, Sent at: {SentTime}",
            request.TaskId,
            request.TaskTitle,
            request.EmailStatus,
            request.SentTimestamp.ToDateTime()
        );

        return await Task.FromResult(new EmailSentResponse
        {
            Success = true,
            Message = $"Email notification for task {request.TaskId} processed successfully"
        });
    }
}
