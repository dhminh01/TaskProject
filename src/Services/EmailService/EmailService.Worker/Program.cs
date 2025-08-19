using EmailService.Application.Handlers;
using EmailService.Application.Interfaces;
using EmailService.Application.Services;
using EmailService.Infrastructure.Clients;
using EmailService.Infrastructure.Services;
using EmailService.Worker;
using MassTransit;
using Shared.Grpc;

var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureAppConfiguration((context, config) =>
{
    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
    config.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true);
    config.AddEnvironmentVariables();
});

builder.ConfigureServices((context, services) =>
{
    var configuration = context.Configuration;

    // Application Services
    services.AddScoped<IEmailService, EmailService.Application.Services.EmailService>();
    services.AddScoped<IEmailTemplateService, EmailTemplateService>();
    services.AddScoped<IGmailService, GmailService>();

    // gRPC Client for TaskService
    services.AddGrpcClient<TaskAckService.TaskAckServiceClient>(options =>
    {
        options.Address = new Uri(configuration["TaskService:GrpcEndpoint"] ?? "https://localhost:7001");
    });
    services.AddScoped<ITaskServiceClient, TaskServiceGrpcClient>();

    // MassTransit Configuration
    services.AddMassTransit(x =>
    {
        // Add consumers
        x.AddConsumer<TaskCreatedEventHandler>();

        x.UsingRabbitMq((context, cfg) =>
        {
            cfg.Host(configuration["RabbitMQ:Host"] ?? "localhost", h =>
            {
                h.Username(configuration["RabbitMQ:Username"] ?? "guest");
                h.Password(configuration["RabbitMQ:Password"] ?? "guest");
            });

            // Configure endpoint for TaskCreated events
            cfg.ReceiveEndpoint("email-service-task-events", e =>
            {
                e.ConfigureConsumer<TaskCreatedEventHandler>(context);

                // Retry configuration
                e.UseMessageRetry(r => r.Intervals(
                    TimeSpan.FromSeconds(30),
                    TimeSpan.FromMinutes(2),
                    TimeSpan.FromMinutes(5)));

                // Error handling
                e.UseInMemoryOutbox();
            });
        });
    });

    // Background Service
    services.AddHostedService<EmailWorkerService>();

    // Logging
    services.AddLogging(builder =>
    {
        builder.AddConsole();
        builder.AddDebug();

        if (context.HostingEnvironment.IsDevelopment())
        {
            builder.SetMinimumLevel(LogLevel.Debug);
        }
    });
});

var host = builder.Build();

// Run the worker service
await host.RunAsync();