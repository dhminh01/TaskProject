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

var builder = WebApplication.CreateBuilder(args);

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