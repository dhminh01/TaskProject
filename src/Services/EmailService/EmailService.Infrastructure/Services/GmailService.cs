using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using EmailService.Application.Interfaces;
using EmailService.Application.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using MimeKit;

namespace EmailService.Infrastructure.Services;

public class GmailService : IGmailService, IDisposable
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<GmailService> _logger;
    private Google.Apis.Gmail.v1.GmailService? _gmailService;
    private UserCredential? _credential;
    private bool _disposed = false;

    public GmailService(IConfiguration configuration, ILogger<GmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<bool> InitializeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Initializing Gmail service...");

            var credentialsPath = _configuration["Gmail:CredentialsPath"] ?? "credentials.json";

            if (!File.Exists(credentialsPath))
            {
                _logger.LogError("Gmail credentials file not found at: {CredentialsPath}", credentialsPath);
                return false;
            }

            using var stream = new FileStream(credentialsPath, FileMode.Open, FileAccess.Read);

            _credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                GoogleClientSecrets.FromStream(stream).Secrets,
                [Google.Apis.Gmail.v1.GmailService.Scope.GmailSend],
                "user",
                cancellationToken);

            _gmailService = new Google.Apis.Gmail.v1.GmailService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = _credential,
                ApplicationName = _configuration["Gmail:ApplicationName"] ?? "TaskDemo Email Service"
            });

            _logger.LogInformation("Gmail service initialized successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Gmail service");
            return false;
        }
    }

    public async Task<EmailSendResult> SendEmailAsync(EmailMessage emailMessage, CancellationToken cancellationToken = default)
    {
        try
        {
            if (_gmailService == null)
            {
                var initResult = await InitializeAsync(cancellationToken);
                if (!initResult)
                {
                    return EmailSendResult.Failure("Gmail service not initialized");
                }
            }

            _logger.LogDebug("Creating MIME message for email to {To}", emailMessage.To);

            var mimeMessage = CreateMimeMessage(emailMessage);
            var raw = Base64UrlEncode(mimeMessage.ToString());

            var gmailMessage = new Message
            {
                Raw = raw
            };

            _logger.LogDebug("Sending email via Gmail API...");

            var request = _gmailService!.Users.Messages.Send(gmailMessage, "me");
            var response = await request.ExecuteAsync(cancellationToken);

            _logger.LogInformation("Email sent successfully via Gmail API, MessageId: {MessageId}", response.Id);

            return EmailSendResult.Success(response.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email via Gmail API to {To}", emailMessage.To);
            return EmailSendResult.Failure($"Gmail API error: {ex.Message}");
        }
    }

    private MimeMessage CreateMimeMessage(EmailMessage emailMessage)
    {
        var message = new MimeMessage();

        var fromEmail = _configuration["Gmail:FromEmail"] ?? throw new InvalidOperationException("Gmail:FromEmail not configured");
        var fromName = emailMessage.FromName ?? _configuration["Gmail:FromName"] ?? "TaskDemo";

        message.From.Add(new MailboxAddress(fromName, fromEmail));
        message.To.Add(MailboxAddress.Parse(emailMessage.To));
        message.Subject = emailMessage.Subject;

        var bodyBuilder = new BodyBuilder();

        if (emailMessage.IsHtml)
        {
            bodyBuilder.HtmlBody = emailMessage.Body;
        }
        else
        {
            bodyBuilder.TextBody = emailMessage.Body;
        }

        message.Body = bodyBuilder.ToMessageBody();

        return message;
    }

    private static string Base64UrlEncode(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        return Convert.ToBase64String(bytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .Replace("=", "");
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _gmailService?.Dispose();
            _disposed = true;
        }
    }
}