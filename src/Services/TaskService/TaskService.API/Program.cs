using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskService.Api.GraphQL.Mutations;
using TaskService.Api.GraphQL.Queries;
using TaskService.Application.Common.Behaviors;
using TaskService.Application.Tasks.Commands;
using TaskService.Application.Tasks.Queries;
using TaskService.Application.Tasks.Validators;
using TaskService.Domain.Interfaces;
using TaskService.Infrastructure.Persistence;
using TaskService.Infrastructure.Repositories;
using MassTransit;
using TaskProject.Proto;
using TaskService.API.Consumers;

var builder = WebApplication.CreateBuilder(args);

// Add gRPC support
builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();

// Configure gRPC client to connect to EmailService
builder.Services.AddGrpcClient<EmailNotification.EmailNotificationClient>(options =>
{
    options.Address = new Uri("https://localhost:5009"); // EmailService gRPC port
}).ConfigureChannel(options =>
{
    options.UnsafeUseInsecureChannelCallCredentials = true;
    options.HttpHandler = new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    };
});

// Register EmailNotificationHandler
builder.Services.AddScoped<EmailConfirmationHandler>();

// Database
builder.Services.AddDbContext<TaskDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositories
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// MediatR
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(CreateTaskCommand).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(GetTaskDetailQuery).Assembly);
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
});

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<CreateTaskCommandValidator>();

// MassTransit
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<EmailConfirmationConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        cfg.ReceiveEndpoint("email-sent-confirmations", e =>
        {
            // Set queue properties
            e.Durable = true;
            e.AutoDelete = false;

            // Configure the consumer
            e.ConfigureConsumer<EmailConfirmationConsumer>(context);
        });

        cfg.PrefetchCount = 5;
    });
});

// GraphQL
builder.Services
    .AddGraphQLServer()
    .AddQueryType<TaskQueries>()     // Add this line
    .AddMutationType<TaskMutations>()
    .AddType<CreateTaskInputType>()
    .AddType<CreateTaskPayloadType>()
    .AddType<CreateTaskResultType>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Allow HTTP/2 without TLS
AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

builder.WebHost.ConfigureKestrel(options =>
{
    // HTTP/1.1 and HTTP/2 for non-TLS endpoint
    options.ListenAnyIP(5008, listenOptions =>
    {
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1AndHttp2;
    });

    // HTTP/1.1 and HTTP/2 for TLS endpoint
    options.ListenAnyIP(7131, listenOptions =>
    {
        listenOptions.UseHttps();
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1AndHttp2;
    });
});

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseCors();
app.UseRouting();

app.MapGraphQL("/api/task");

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<TaskDbContext>();
    await context.Database.EnsureCreatedAsync();
}

await app.RunAsync();