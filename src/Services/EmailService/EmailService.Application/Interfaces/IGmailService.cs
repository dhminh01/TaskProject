using EmailService.Application.Models;

namespace EmailService.Application.Interfaces;

public interface IGmailService
{
    Task<EmailSendResult> SendEmailAsync(EmailMessage emailMessage, CancellationToken cancellationToken = default);
    Task<bool> InitializeAsync(CancellationToken cancellationToken = default);
}