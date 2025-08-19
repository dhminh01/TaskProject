using MassTransit;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using EmailService.Application.Interfaces;

namespace EmailService.Worker;

public class EmailWorkerService : BackgroundService
{
    private readonly IBusControl _busControl;
    private readonly IGmailService _gmailService;
    private readonly ILogger<EmailWorkerService> _logger;

    public EmailWorkerService(
        IBusControl busControl,
        IGmailService gmailService,
        ILogger<EmailWorkerService> logger)
    {
        _busControl = busControl;
        _gmailService = gmailService;
        _logger = logger;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Email Worker Service is starting...");

        // Initialize Gmail service
        var gmailInitialized = await _gmailService.InitializeAsync(cancellationToken);
        if (!gmailInitialized)
        {
            _logger.LogError("Failed to initialize Gmail service. Worker service will not start.");
            throw new InvalidOperationException("Gmail service initialization failed");
        }

        // Start MassTransit bus
        await _busControl.StartAsync(cancellationToken);
        _logger.LogInformation("MassTransit bus started successfully");

        await base.StartAsync(cancellationToken);
        _logger.LogInformation("Email Worker Service started successfully");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Email Worker Service is running and listening for messages...");

        try
        {
            // Keep the service running and listening for messages
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

                // Optional: Add health check or periodic maintenance tasks here
                _logger.LogDebug("Email Worker Service heartbeat - {Timestamp}", DateTimeOffset.UtcNow);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Email Worker Service execution was cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred in Email Worker Service execution");
            throw;
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Email Worker Service is stopping...");

        await _busControl.StopAsync(cancellationToken);
        _logger.LogInformation("MassTransit bus stopped");

        await base.StopAsync(cancellationToken);
        _logger.LogInformation("Email Worker Service stopped successfully");
    }
}