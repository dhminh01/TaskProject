using EmailService.Application.Enums;
using EmailService.Application.Interfaces;
using EmailService.Application.Models;
using Microsoft.Extensions.Logging;

namespace EmailService.Application.Services;

public class EmailService : IEmailService
{
    private readonly IGmailService _gmailService;
    private readonly IEmailTemplateService _templateService;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        IGmailService gmailService,
        IEmailTemplateService templateService,
        ILogger<EmailService> logger)
    {
        _gmailService = gmailService;
        _templateService = templateService;
        _logger = logger;
    }

    public async Task<EmailSendResult> SendEmailAsync(EmailMessage emailMessage, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Sending email to {To} with subject '{Subject}'", emailMessage.To, emailMessage.Subject);

            var result = await _gmailService.SendEmailAsync(emailMessage, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Email sent successfully to {To}, MessageId: {MessageId}",
                    emailMessage.To, result.MessageId);
            }
            else
            {
                _logger.LogError("Failed to send email to {To}: {Error}",
                    emailMessage.To, result.ErrorMessage);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while sending email to {To}", emailMessage.To);
            return EmailSendResult.Failure($"Exception: {ex.Message}");
        }
    }

    public async Task<EmailSendResult> SendTaskNotificationAsync(
        TaskEmailData taskData,
        EmailTemplate template,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Generating email from template {Template} for Task {TaskId}",
                template, taskData.TaskId);

            var emailMessage = await _templateService.GenerateEmailFromTemplateAsync(
                taskData, template, cancellationToken);

            return await SendEmailAsync(emailMessage, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send task notification for Task {TaskId} with template {Template}",
                taskData.TaskId, template);
            return EmailSendResult.Failure($"Template processing failed: {ex.Message}");
        }
    }
}