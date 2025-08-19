using EmailService.Application.Enums;
using EmailService.Application.Models;

namespace EmailService.Application.Interfaces;

public interface IEmailTemplateService
{
    Task<EmailMessage> GenerateEmailFromTemplateAsync(TaskEmailData taskData, EmailTemplate template, CancellationToken cancellationToken = default);
}