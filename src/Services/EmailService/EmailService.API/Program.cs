
using EmailService.API.Consumers;
using EmailService.API.Services;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();


// Configure services
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<TaskCreatedEventConsumer>();
    x.AddConsumer<TaskUpdatedEventConsumer>();
    x.AddConsumer<TaskDeletedEventConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        cfg.ReceiveEndpoint("email-service-task-created-v2", e =>
        {
            // Set queue properties before configuring the consumer
            e.Durable = true;
            e.AutoDelete = false;

            // Set message TTL to 1 hour (using integer milliseconds)
            e.SetQueueArgument("x-message-ttl", 3600000);

            // Enable dead letter queue
            e.SetQueueArgument("x-dead-letter-exchange", "email-service-dlx");
            e.SetQueueArgument("x-dead-letter-routing-key", "email-service-dlq");

            // Configure retry policy
            e.UseMessageRetry(r =>
            {
                r.Immediate(3); // Retry 3 times immediately
                r.SetRetryPolicy(policy => policy.Interval(3, TimeSpan.FromSeconds(5))); // Then retry 3 more times with 5 second intervals
            });

            e.ConfigureConsumer<TaskCreatedEventConsumer>(context);
        });

        cfg.ReceiveEndpoint("email-service-task-updated-v2", e =>
        {
            // Set queue properties before configuring the consumer
            e.Durable = true;
            e.AutoDelete = false;

            // Set message TTL to 1 hour (using integer milliseconds)
            e.SetQueueArgument("x-message-ttl", 3600000);

            // Enable dead letter queue
            e.SetQueueArgument("x-dead-letter-exchange", "email-service-dlx");
            e.SetQueueArgument("x-dead-letter-routing-key", "email-service-dlq");

            // Configure retry policy
            e.UseMessageRetry(r =>
            {
                r.Immediate(3); // Retry 3 times immediately
                r.SetRetryPolicy(policy => policy.Interval(3, TimeSpan.FromSeconds(5))); // Then retry 3 more times with 5 second intervals
            });

            e.ConfigureConsumer<TaskUpdatedEventConsumer>(context);
        });

        cfg.ReceiveEndpoint("email-service-task-deleted-v1", e =>
        {
            // Set queue properties before configuring the consumer
            e.Durable = true;
            e.AutoDelete = false;

            // Set message TTL to 1 hour (using integer milliseconds)
            e.SetQueueArgument("x-message-ttl", 3600000);

            // Enable dead letter queue
            e.SetQueueArgument("x-dead-letter-exchange", "email-service-dlx");
            e.SetQueueArgument("x-dead-letter-routing-key", "email-service-dlq");

            // Configure retry policy
            e.UseMessageRetry(r =>
            {
                r.Immediate(3); // Retry 3 times immediately
                r.SetRetryPolicy(policy => policy.Interval(3, TimeSpan.FromSeconds(5))); // Then retry 3 more times with 5 second intervals
            });

            e.ConfigureConsumer<TaskDeletedEventConsumer>(context);
        });

        // Configure dead letter queue
        cfg.ReceiveEndpoint("email-service-dlq", e =>
        {
            e.Durable = true;
            e.AutoDelete = false;
        });
    });
});

// Add logging
builder.Services.AddLogging();

// Add Gmail email service as singleton to prevent multiple token file access
builder.Services.AddSingleton<IEmailService, GmailEmailService>();

// Configure Kestrel to listen on both HTTP and HTTPS
builder.WebHost.ConfigureKestrel(options =>
{
    // Setup HTTP/2 endpoint with TLS on port 5009
    options.ListenLocalhost(5009, o =>
    {
        o.UseHttps();
        o.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
app.MapGrpcService<EmailNotificationService>();
app.MapGrpcReflectionService();

await app.RunAsync();
