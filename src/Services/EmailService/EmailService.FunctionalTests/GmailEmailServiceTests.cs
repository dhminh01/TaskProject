using EmailService.API.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace EmailService.FunctionalTests
{
    public class GmailEmailServiceTests : IDisposable
    {
        private readonly IConfiguration _configuration;
        private readonly Mock<ILogger<GmailEmailService>> _loggerMock;
        private readonly IEmailService _emailService;
        private readonly Dictionary<string, string?> _originalEnvVars;

        public GmailEmailServiceTests()
        {
            // Store original environment variables
            _originalEnvVars = new Dictionary<string, string?>
            {
                { "GMAIL_CLIENT_ID", Environment.GetEnvironmentVariable("GMAIL_CLIENT_ID") },
                { "GMAIL_CLIENT_SECRET", Environment.GetEnvironmentVariable("GMAIL_CLIENT_SECRET") },
                { "GMAIL_FROM_EMAIL", Environment.GetEnvironmentVariable("GMAIL_FROM_EMAIL") }
            };

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

            _loggerMock = new Mock<ILogger<GmailEmailService>>();
            _emailService = new GmailEmailService(_configuration, _loggerMock.Object);
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
        //     var configDict = new Dictionary<string, string?>
        //     {
        //         {"GMAIL_CLIENT_ID", null},
        //         {"GMAIL_CLIENT_SECRET", "test_client_secret"},
        //         {"GMAIL_FROM_EMAIL", "test@example.com"}
        //     };

        //     var config = new ConfigurationBuilder()
        //         .AddInMemoryCollection(configDict)
        //         .Build();

        //     Environment.SetEnvironmentVariable("GMAIL_CLIENT_ID", null);

        //     // Act & Assert
        //     var exception = Assert.Throws<ArgumentException>(() =>
        //         new GmailEmailService(config, _loggerMock.Object));

        //     Assert.Contains("GMAIL_CLIENT_ID environment variable is missing", exception.Message);
        // }

        // [Fact]
        // public void Constructor_MissingClientSecret_ThrowsArgumentException()
        // {
        //     // Arrange
        //     var configDict = new Dictionary<string, string?>
        //     {
        //         {"GMAIL_CLIENT_ID", "test_client_id"},
        //         {"GMAIL_CLIENT_SECRET", null},
        //         {"GMAIL_FROM_EMAIL", "test@example.com"}
        //     };

        //     var config = new ConfigurationBuilder()
        //         .AddInMemoryCollection(configDict)
        //         .Build();

        //     Environment.SetEnvironmentVariable("GMAIL_CLIENT_SECRET", null);

        //     // Act & Assert
        //     var exception = Assert.Throws<ArgumentException>(() =>
        //         new GmailEmailService(config, _loggerMock.Object));

        //     Assert.Contains("GMAIL_CLIENT_SECRET environment variable is missing", exception.Message);
        // }

        // [Fact]
        // public void Constructor_MissingFromEmail_ThrowsArgumentException()
        // {
        //     // Arrange
        //     var configDict = new Dictionary<string, string?>
        //     {
        //         {"GMAIL_CLIENT_ID", "test_client_id"},
        //         {"GMAIL_CLIENT_SECRET", "test_client_secret"},
        //         {"GMAIL_FROM_EMAIL", null}
        //     };

        //     var config = new ConfigurationBuilder()
        //         .AddInMemoryCollection(configDict)
        //         .Build();

        //     Environment.SetEnvironmentVariable("GMAIL_FROM_EMAIL", null);

        //     // Act & Assert
        //     var exception = Assert.Throws<ArgumentException>(() =>
        //         new GmailEmailService(config, _loggerMock.Object));

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
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
                Times.Never);
        }

        public void Dispose()
        {
            // Restore original environment variables
            foreach (var envVar in _originalEnvVars)
            {
                Environment.SetEnvironmentVariable(envVar.Key, envVar.Value);
            }
        }
    }
}