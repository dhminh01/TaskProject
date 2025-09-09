using Moq;
using TaskService.Application.Tasks.Commands.CreateTask;
using TaskService.Domain.Entities;
using TaskService.Domain.Events;
using TaskService.Domain.Interfaces;
using TaskService.Domain.Common.Exceptions;
using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging;
using TaskService.Application.Tasks.Commands;
using TaskService.Application.Tasks.Validators;
using FluentValidation;

namespace TaskService.FunctionalTests.Handlers;

public class CreateTaskCommandHandlerTest
{
    private readonly Mock<ITaskRepository> _taskRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IPublishEndpoint> _publishEndpointMock;
    private readonly Mock<ILogger<CreateTaskCommandHandler>> _loggerMock;
    private readonly CreateTaskCommandHandler _handler;
    private readonly IValidator<CreateTaskCommand> _validator;

    public CreateTaskCommandHandlerTest()
    {
        _taskRepositoryMock = new Mock<ITaskRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _publishEndpointMock = new Mock<IPublishEndpoint>();
        _loggerMock = new Mock<ILogger<CreateTaskCommandHandler>>();
        _validator = new CreateTaskCommandValidator();
        _handler = new CreateTaskCommandHandler(
            _taskRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _publishEndpointMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateTaskAndPublishEvent()
    {
        // Arrange
        var command = new CreateTaskCommand
        {
            Title = "Test Task",
            Description = "Test Description",
            DueDate = DateTime.UtcNow.AddDays(1)
        };

        _taskRepositoryMock.Setup(x => x.GetByTitleAsync(command.Title, It.IsAny<CancellationToken>()))
            .ReturnsAsync(null as TaskItem);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be(command.Title);
        result.Description.Should().Be(command.Description);
        result.DueDate.Should().Be(command.DueDate);

        _taskRepositoryMock.Verify(x => x.AddAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _publishEndpointMock.Verify(x => x.Publish(It.IsAny<TaskCreatedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public async Task Handle_DuplicateTitle_ShouldThrowDuplicateTaskTitleException()
    {
        // Arrange
        var command = new CreateTaskCommand
        {
            Title = "Test Task",
            Description = "Test Description",
            DueDate = DateTime.UtcNow.AddDays(1)
        };

        var existingTask = new TaskItem("Test Task", "Existing Description", DateTime.UtcNow.AddDays(2));
        _taskRepositoryMock.Setup(x => x.GetByTitleAsync(command.Title, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTask);

        // Act & Assert
        await Assert.ThrowsAsync<DuplicateTaskTitleException>(() => _handler.Handle(command, CancellationToken.None));
        _taskRepositoryMock.Verify(x => x.AddAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(-1)] // Yesterday
    [InlineData(-7)] // A week ago
    [InlineData(-30)] // A month ago
    public async Task Handle_PastDueDate_ShouldFailValidation(int daysOffset)
    {
        // Arrange
        var pastDate = DateTime.UtcNow.Date.AddDays(daysOffset);
        var command = new CreateTaskCommand
        {
            Title = "Test Task",
            Description = "Test Description",
            DueDate = pastDate
        };

        // Act
        var validationResult = await _validator.ValidateAsync(command);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().Contain(e => e.PropertyName == "DueDate" &&
            e.ErrorMessage.Contains("future date", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrowsException_ShouldPropagateException()
    {
        // Arrange
        var command = new CreateTaskCommand
        {
            Title = "Test Task",
            Description = "Test Description",
            DueDate = DateTime.UtcNow.AddDays(1)
        };

        _taskRepositoryMock.Setup(x => x.GetByTitleAsync(command.Title, It.IsAny<CancellationToken>()))
            .ReturnsAsync(null as TaskItem);
        _taskRepositoryMock.Setup(x => x.AddAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenPublishEndpointThrowsException_ShouldPropagateException()
    {
        // Arrange
        var command = new CreateTaskCommand
        {
            Title = "Test Task",
            Description = "Test Description",
            DueDate = DateTime.UtcNow.AddDays(1)
        };

        _taskRepositoryMock.Setup(x => x.GetByTitleAsync(command.Title, It.IsAny<CancellationToken>()))
            .ReturnsAsync(null as TaskItem);
        _publishEndpointMock.Setup(x => x.Publish(It.IsAny<TaskCreatedEvent>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Publisher error"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }
}