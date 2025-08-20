
using EmailService.API.Consumers;
using EmailService.API.Services;
using MassTransit;
using TaskProject.EmailService;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();

// EmailService is the gRPC server, so we don't need gRPC client configuration here

// Configure services
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<TaskCreatedEventConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        cfg.ReceiveEndpoint("email-service-task-created", e =>
        {
            e.ConfigureConsumer<TaskCreatedEventConsumer>(context);
        });
    });
});

// Add Gmail email service
builder.Services.AddTransient<IEmailService, GmailEmailService>();

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
