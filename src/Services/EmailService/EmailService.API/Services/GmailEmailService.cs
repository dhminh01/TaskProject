using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using MimeKit;

namespace EmailService.API.Services
{
    public class GmailEmailService : IEmailService, IDisposable
    {
        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private static GmailService? _sharedGmailService;
        private readonly string _fromEmail;
        private readonly ILogger<GmailEmailService> _logger;
        private bool _disposed;

        public GmailEmailService(IConfiguration configuration, ILogger<GmailEmailService> logger)
        {
            _logger = logger;

            try
            {
                // Load .env file
                DotNetEnv.Env.Load();

                _fromEmail = Environment.GetEnvironmentVariable("GMAIL_FROM_EMAIL")
                    ?? throw new ArgumentException("GMAIL_FROM_EMAIL environment variable is missing");

                InitializeGmailServiceAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing GmailEmailService");
                throw;
            }
        }

        private async Task InitializeGmailServiceAsync()
        {
            if (_sharedGmailService != null) return;

            await _semaphore.WaitAsync();
            try
            {
                if (_sharedGmailService != null) return;

                string[] Scopes = { GmailService.Scope.GmailSend };
                string ApplicationName = "Task Project";

                var clientId = Environment.GetEnvironmentVariable("GMAIL_CLIENT_ID")
                    ?? throw new ArgumentException("GMAIL_CLIENT_ID environment variable is missing");
                var clientSecret = Environment.GetEnvironmentVariable("GMAIL_CLIENT_SECRET")
                    ?? throw new ArgumentException("GMAIL_CLIENT_SECRET environment variable is missing");

                var clientSecrets = new ClientSecrets
                {
                    ClientId = clientId,
                    ClientSecret = clientSecret
                };

                var tokenFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "token.json");
                Directory.CreateDirectory(tokenFolder);

                // The file token.json stores the user's access and refresh tokens
                var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    clientSecrets,
                    Scopes,
                    _fromEmail,
                    CancellationToken.None,
                    new FileDataStore(tokenFolder, true));

                _sharedGmailService = new GmailService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = ApplicationName
                });
            }
            finally
            {
                _semaphore.Release();
            }
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

            if (_sharedGmailService == null)
            {
                throw new InvalidOperationException("Gmail service is not initialized");
            }

            try
            {
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

                    await _sharedGmailService.Users.Messages.Send(gmail, "me").ExecuteAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email to {EmailAddress}", to);
                throw;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                _semaphore.Dispose();
            }

            _disposed = true;
        }
    }
}
