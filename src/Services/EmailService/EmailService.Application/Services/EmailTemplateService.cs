using EmailService.Application.Enums;
using EmailService.Application.Interfaces;
using EmailService.Application.Models;
using Microsoft.Extensions.Configuration;

namespace EmailService.Application.Services;

public class EmailTemplateService : IEmailTemplateService
{
    private readonly IConfiguration _configuration;

    public EmailTemplateService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<EmailMessage> GenerateEmailFromTemplateAsync(
        TaskEmailData taskData,
        EmailTemplate template,
        CancellationToken cancellationToken = default)
    {
        return template switch
        {
            EmailTemplate.TaskCreated => await GenerateTaskCreatedEmailAsync(taskData, cancellationToken),
            EmailTemplate.TaskReminder => await GenerateTaskReminderEmailAsync(taskData, cancellationToken),
            EmailTemplate.TaskOverdue => await GenerateTaskOverdueEmailAsync(taskData, cancellationToken),
            _ => throw new ArgumentException($"Unsupported email template: {template}")
        };
    }

    private async Task<EmailMessage> GenerateTaskCreatedEmailAsync(TaskEmailData taskData, CancellationToken cancellationToken)
    {
        var dueDateText = taskData.DueDate.HasValue
            ? taskData.DueDate.Value.ToString("MMMM dd, yyyy 'at' hh:mm tt")
            : "No due date specified";

        var subject = $"New Task Created: {taskData.Title}";

        var body = $@"
            <html>
            <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <h2 style='color: #2c3e50; border-bottom: 2px solid #3498db; padding-bottom: 10px;'>
                        🎯 New Task Created
                    </h2>
                    
                    <div style='background-color: #f8f9fa; padding: 20px; border-radius: 5px; margin: 20px 0;'>
                        <h3 style='color: #2c3e50; margin-top: 0;'>{taskData.Title}</h3>
                        <p style='margin: 10px 0;'><strong>Description:</strong></p>
                        <p style='background-color: white; padding: 15px; border-left: 4px solid #3498db; margin: 10px 0;'>
                            {taskData.Description}
                        </p>
                        <p style='margin: 10px 0;'><strong>Due Date:</strong> {dueDateText}</p>
                        <p style='margin: 10px 0;'><strong>Created:</strong> {taskData.DateCreated:MMMM dd, yyyy 'at' hh:mm tt}</p>
                    </div>
                    
                    <div style='background-color: #e8f6f3; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                        <p style='margin: 0; font-size: 14px; color: #27ae60;'>
                            ✅ This task has been successfully created and is ready for action!
                        </p>
                    </div>
                    
                    <hr style='border: none; border-top: 1px solid #eee; margin: 30px 0;'>
                    <p style='font-size: 12px; color: #7f8c8d; text-align: center;'>
                        This is an automated notification from TaskDemo System<br>
                        Task ID: {taskData.TaskId}
                    </p>
                </div>
            </body>
            </html>";

        return new EmailMessage
        {
            To = taskData.RecipientEmail,
            Subject = subject,
            Body = body,
            FromName = _configuration["Email:FromName"] ?? "TaskDemo System",
            IsHtml = true
        };
    }

    private async Task<EmailMessage> GenerateTaskReminderEmailAsync(TaskEmailData taskData, CancellationToken cancellationToken)
    {
        var dueDateText = taskData.DueDate.HasValue
            ? taskData.DueDate.Value.ToString("MMMM dd, yyyy 'at' hh:mm tt")
            : "No due date specified";

        var subject = $"⏰ Task Reminder: {taskData.Title}";

        var body = $@"
            <html>
            <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <h2 style='color: #f39c12; border-bottom: 2px solid #f39c12; padding-bottom: 10px;'>
                        ⏰ Task Reminder
                    </h2>
                    
                    <div style='background-color: #fef9e7; padding: 20px; border-radius: 5px; margin: 20px 0; border-left: 4px solid #f39c12;'>
                        <h3 style='color: #2c3e50; margin-top: 0;'>{taskData.Title}</h3>
                        <p style='margin: 10px 0;'><strong>Description:</strong></p>
                        <p style='background-color: white; padding: 15px; border-radius: 3px; margin: 10px 0;'>
                            {taskData.Description}
                        </p>
                        <p style='margin: 10px 0;'><strong>Due Date:</strong> {dueDateText}</p>
                    </div>
                    
                    <div style='background-color: #fff3cd; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                        <p style='margin: 0; font-size: 14px; color: #856404;'>
                            📋 Don't forget to complete this task!
                        </p>
                    </div>
                    
                    <hr style='border: none; border-top: 1px solid #eee; margin: 30px 0;'>
                    <p style='font-size: 12px; color: #7f8c8d; text-align: center;'>
                        This is an automated reminder from TaskDemo System<br>
                        Task ID: {taskData.TaskId}
                    </p>
                </div>
            </body>
            </html>";

        return new EmailMessage
        {
            To = taskData.RecipientEmail,
            Subject = subject,
            Body = body,
            FromName = _configuration["Email:FromName"] ?? "TaskDemo System",
            IsHtml = true
        };
    }

    private async Task<EmailMessage> GenerateTaskOverdueEmailAsync(TaskEmailData taskData, CancellationToken cancellationToken)
    {
        var dueDateText = taskData.DueDate.HasValue
            ? taskData.DueDate.Value.ToString("MMMM dd, yyyy 'at' hh:mm tt")
            : "No due date specified";

        var subject = $"🚨 OVERDUE Task: {taskData.Title}";

        var body = $@"
            <html>
            <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <h2 style='color: #e74c3c; border-bottom: 2px solid #e74c3c; padding-bottom: 10px;'>
                        🚨 OVERDUE Task
                    </h2>
                    
                    <div style='background-color: #fdf2f2; padding: 20px; border-radius: 5px; margin: 20px 0; border-left: 4px solid #e74c3c;'>
                        <h3 style='color: #2c3e50; margin-top: 0;'>{taskData.Title}</h3>
                        <p style='margin: 10px 0;'><strong>Description:</strong></p>
                        <p style='background-color: white; padding: 15px; border-radius: 3px; margin: 10px 0;'>
                            {taskData.Description}
                        </p>
                        <p style='margin: 10px 0; color: #e74c3c;'><strong>Was Due:</strong> {dueDateText}</p>
                    </div>
                    
                    <div style='background-color: #f8d7da; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                        <p style='margin: 0; font-size: 14px; color: #721c24;'>
                            ⚠️ This task is overdue and requires immediate attention!
                        </p>
                    </div>
                    
                    <hr style='border: none; border-top: 1px solid #eee; margin: 30px 0;'>
                    <p style='font-size: 12px; color: #7f8c8d; text-align: center;'>
                        This is an automated overdue notification from TaskDemo System<br>
                        Task ID: {taskData.TaskId}
                    </p>
                </div>
            </body>
            </html>";

        return new EmailMessage
        {
            To = taskData.RecipientEmail,
            Subject = subject,
            Body = body,
            FromName = _configuration["Email:FromName"] ?? "TaskDemo System",
            IsHtml = true
        };
    }
}