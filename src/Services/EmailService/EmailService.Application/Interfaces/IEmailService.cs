using EmailService.Application.Enums;
using EmailService.Application.Models;

namespace EmailService.Application.Interfaces;

public interface IEmailService
{
    Task<EmailSendResult> SendEmailAsync(EmailMessage emailMessage, CancellationToken cancellationToken = default);
    Task<EmailSendResult> SendTaskNotificationAsync(TaskEmailData taskData, EmailTemplate template, CancellationToken cancellationToken = default);
}
