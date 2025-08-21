using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using MimeKit;

namespace EmailService.API.Services
{
    public class GmailEmailService : IEmailService
    {
        private readonly GmailService _gmailService;
        private readonly string _fromEmail;

        public GmailEmailService(IConfiguration configuration)
        {
            // Load .env file
            DotNetEnv.Env.Load();

            string[] Scopes = { GmailService.Scope.GmailSend };
            string ApplicationName = "Task Project";

            var clientId = Environment.GetEnvironmentVariable("GMAIL_CLIENT_ID")
                ?? throw new ArgumentException("GMAIL_CLIENT_ID environment variable is missing");
            var clientSecret = Environment.GetEnvironmentVariable("GMAIL_CLIENT_SECRET")
                ?? throw new ArgumentException("GMAIL_CLIENT_SECRET environment variable is missing");
            _fromEmail = Environment.GetEnvironmentVariable("GMAIL_FROM_EMAIL")
                ?? throw new ArgumentException("GMAIL_FROM_EMAIL environment variable is missing");

            var clientSecrets = new ClientSecrets
            {
                ClientId = clientId,
                ClientSecret = clientSecret
            };

            // The file token.json stores the user's access and refresh tokens
            var credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                clientSecrets,
                Scopes,
                _fromEmail,
                CancellationToken.None,
                new FileDataStore("token.json", true)).Result;

            _gmailService = new GmailService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName
            });
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            if (string.IsNullOrEmpty(to))
                throw new ArgumentException("Recipient address is required", nameof(to));
            if (string.IsNullOrEmpty(subject))
                throw new ArgumentException("Subject is required", nameof(subject));
            if (string.IsNullOrEmpty(body))
                throw new ArgumentException("Body is required", nameof(body));

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Task Service", _fromEmail));
            message.To.Add(new MailboxAddress("", to));
            message.Subject = subject;

            message.Body = new TextPart("plain")
            {
                Text = body
            };

            using (var ms = new MemoryStream())
            {
                await message.WriteToAsync(ms);
                var gmail = new Message
                {
                    Raw = Convert.ToBase64String(ms.ToArray())
                        .Replace('+', '-')
                        .Replace('/', '_')
                        .Replace("=", "")
                };

                await _gmailService.Users.Messages.Send(gmail, "me").ExecuteAsync();
            }
        }
    }
}
