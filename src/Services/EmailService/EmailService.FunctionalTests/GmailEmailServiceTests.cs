using EmailService.API.Services;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace EmailService.FunctionalTests
{
    public class GmailEmailServiceTests
    {
        private readonly IConfiguration _configuration;
        private IEmailService _emailService;

        public GmailEmailServiceTests()
        {
            var configDictionary = new Dictionary<string, string?>
            {
                {"GMAIL_CLIENT_ID", "test_client_id"},
                {"GMAIL_CLIENT_SECRET", "test_client_secret"},
                {"GMAIL_FROM_EMAIL", "test@example.com"}
            };

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configDictionary)
                .Build();

            Environment.SetEnvironmentVariable("GMAIL_CLIENT_ID", "test_client_id");
            Environment.SetEnvironmentVariable("GMAIL_CLIENT_SECRET", "test_client_secret");
            Environment.SetEnvironmentVariable("GMAIL_FROM_EMAIL", "test@example.com");

            _emailService = new GmailEmailService(_configuration);
        }

        [Fact]
        public async Task SendEmailAsync_EmptyRecipient_ThrowsArgumentException()
        {
            // Arrange
            var toEmail = string.Empty;
            var subject = "Test Email";
            var body = "This is a test email.";

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await _emailService.SendEmailAsync(toEmail, subject, body);
            });

            Assert.Equal("to", exception.ParamName);
            Assert.Contains("Recipient address is required", exception.Message);
        }

        [Fact]
        public async Task SendEmailAsync_NullSubject_ThrowsArgumentException()
        {
            // Arrange
            var toEmail = "test@example.com";
            string? subject = null;
            var body = "This is a test email.";

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await _emailService.SendEmailAsync(toEmail, subject!, body);
            });

            Assert.Equal("subject", exception.ParamName);
            Assert.Contains("Subject is required", exception.Message);
        }

        [Fact]
        public async Task SendEmailAsync_NullBody_ThrowsArgumentException()
        {
            // Arrange
            var toEmail = "test@example.com";
            var subject = "Test Email";
            string? body = null;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await _emailService.SendEmailAsync(toEmail, subject, body!);
            });

            Assert.Equal("body", exception.ParamName);
            Assert.Contains("Body is required", exception.Message);
        }

        [Fact]
        public async Task SendEmailAsync_EmptySubject_ThrowsArgumentException()
        {
            // Arrange
            var toEmail = "test@example.com";
            var subject = string.Empty;
            var body = "Test body";

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await _emailService.SendEmailAsync(toEmail, subject, body);
            });

            Assert.Equal("subject", exception.ParamName);
            Assert.Contains("Subject is required", exception.Message);
        }

        [Fact]
        public async Task SendEmailAsync_EmptyBody_ThrowsArgumentException()
        {
            // Arrange
            var toEmail = "test@example.com";
            var subject = "Test subject";
            var body = string.Empty;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await _emailService.SendEmailAsync(toEmail, subject, body);
            });

            Assert.Equal("body", exception.ParamName);
            Assert.Contains("Body is required", exception.Message);
        }

        // [Fact]
        // public void Constructor_MissingClientId_ThrowsArgumentException()
        // {
        //     // Arrange
        //     Environment.SetEnvironmentVariable("GMAIL_CLIENT_ID", null);

        //     // Act & Assert
        //     var exception = Assert.Throws<ArgumentException>(() => new GmailEmailService(_configuration));
        //     Assert.Contains("GMAIL_CLIENT_ID environment variable is missing", exception.Message);
        // }

        // [Fact]
        // public void Constructor_MissingClientSecret_ThrowsArgumentException()
        // {
        //     // Arrange
        //     Environment.SetEnvironmentVariable("GMAIL_CLIENT_SECRET", null);

        //     // Act & Assert
        //     var exception = Assert.Throws<ArgumentException>(() => new GmailEmailService(_configuration));
        //     Assert.Contains("GMAIL_CLIENT_SECRET environment variable is missing", exception.Message);
        // }

        // [Fact]
        // public void Constructor_MissingFromEmail_ThrowsArgumentException()
        // {
        //     // Arrange
        //     Environment.SetEnvironmentVariable("GMAIL_FROM_EMAIL", null);

        //     // Act & Assert
        //     var exception = Assert.Throws<ArgumentException>(() => new GmailEmailService(_configuration));
        //     Assert.Contains("GMAIL_FROM_EMAIL environment variable is missing", exception.Message);
        // }

        [Fact]
        public async Task SendEmailAsync_ValidParameters_NoException()
        {
            // Arrange
            var toEmail = "test@example.com";
            var subject = "Test Email";
            var body = "This is a test email body";

            // Act & Assert
            await _emailService.SendEmailAsync(toEmail, subject, body);
            // If no exception is thrown, the test passes
        }
    }
}