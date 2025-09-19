using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskService.Application.Common.Behaviors;
using TaskService.Domain.Interfaces;
using TaskService.Infrastructure.Persistence;
using TaskService.Infrastructure.Repositories;
using MassTransit;
using TaskProject.Proto;
using TaskService.API.Consumers;
using TaskService.Application;
using TaskService.API.Extensions;
using TaskService.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add gRPC support
builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();

// Configure gRPC client to connect to EmailService
builder.Services.AddGrpcClient<EmailNotification.EmailNotificationClient>(options =>
{
    options.Address = new Uri("http://emailservice:5009"); // EmailService gRPC port in Docker network
});

// Register EmailNotificationHandler
builder.Services.AddScoped<EmailConfirmationHandler>();

// Database
builder.Services.AddDbContext<TaskDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// Repositories
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// MediatR
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(AssemblyAnchor).Assembly);
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
});

// FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(AssemblyAnchor).Assembly);

// MassTransit
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<EmailConfirmationConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        var host = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost";
        var username = Environment.GetEnvironmentVariable("RABBITMQ_USERNAME") ?? "guest";
        var password = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD") ?? "guest";

        cfg.Host(host, "/", h =>
        {
            h.Username(username);
            h.Password(password);
        });

        cfg.ReceiveEndpoint("email-sent-confirmations-v2", e =>
        {
            // Set queue properties before any other configuration
            e.Durable = true;
            e.AutoDelete = false;

            // Set message TTL to 1 hour (using integer milliseconds)
            e.SetQueueArgument("x-message-ttl", 3600000);

            // Enable dead letter queue
            e.SetQueueArgument("x-dead-letter-exchange", "task-service-dlx");
            e.SetQueueArgument("x-dead-letter-routing-key", "task-service-dlq");

            // Configure retry policy
            e.UseMessageRetry(r =>
            {
                r.Immediate(3); // Retry 3 times immediately
                r.SetRetryPolicy(policy => policy.Interval(3, TimeSpan.FromSeconds(5))); // Then retry 3 more times with 5 second intervals
            });

            // Configure the consumer
            e.ConfigureConsumer<EmailConfirmationConsumer>(context);
        });

        // Configure dead letter queue
        cfg.ReceiveEndpoint("task-service-dlq", e =>
        {
            e.Durable = true;
            e.AutoDelete = false;
        });

        // Set prefetch count and configure message delivery
        cfg.PrefetchCount = 5;

        // Configure publisher retry using the correct method
        cfg.UseDelayedRedelivery(r => r.Intervals(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(15)));
        cfg.UseMessageRetry(r => r.Immediate(3));
    });
});

// GraphQL
builder.Services.AddGraphQLServices(builder.Environment);

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy =>
        {
            policy.WithOrigins("http://localhost:5173") // your React app URL
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

// Allow HTTP/2 without TLS
AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

builder.WebHost.ConfigureKestrel(options =>
{
    // HTTP endpoint for both Http1 and Http2
    options.ListenAnyIP(5008, listenOptions =>
    {
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1AndHttp2;
    });
});

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseCors("AllowReactApp");
app.UseRouting();

app.UseMiddleware<ErrorHandlingMiddleware>();

app.MigrateDb();

// Configure GraphQL endpoint with options
app.MapGraphQL("/api/task").WithOptions(new HotChocolate.AspNetCore.GraphQLServerOptions
{
    Tool = { Enable = builder.Environment.IsDevelopment() }
});

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<TaskDbContext>();
    await context.Database.EnsureCreatedAsync();
}

await app.RunAsync();