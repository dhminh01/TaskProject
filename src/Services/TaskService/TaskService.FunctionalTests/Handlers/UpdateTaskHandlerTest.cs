using FluentAssertions;
using FluentValidation;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using TaskService.Application.Tasks.Commands;
using TaskService.Application.Tasks.Commands.Handlers;
using TaskService.Application.Tasks.Validators;
using TaskService.Domain.Common.Exceptions;
using TaskService.Domain.Entities;
using TaskService.Domain.Events;
using TaskService.Domain.Interfaces;

namespace TaskService.FunctionalTests.Handlers;

public class UpdateTaskHandlerTest
{
    private readonly Mock<ITaskRepository> _taskRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IPublishEndpoint> _publishEndpointMock;
    private readonly Mock<ILogger<UpdateTaskCommandHandler>> _loggerMock;
    private readonly UpdateTaskCommandHandler _handler;
    private readonly IValidator<UpdateTaskCommand> _validator;

    public UpdateTaskHandlerTest()
    {
        _taskRepositoryMock = new Mock<ITaskRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _publishEndpointMock = new Mock<IPublishEndpoint>();
        _loggerMock = new Mock<ILogger<UpdateTaskCommandHandler>>();
        _validator = new UpdateTaskCommandValidator();
        _handler = new UpdateTaskCommandHandler(
            _taskRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _publishEndpointMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldUpdateTaskAndPublishEvent()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var existingTask = new TaskItem("Old Title", "Old Description", DateTime.UtcNow.AddDays(2));

        var command = new UpdateTaskCommand
        {
            Id = taskId,
            Title = "Updated Title",
            Description = "Updated Description",
            DueDate = DateTime.UtcNow.AddDays(3)
        };

        _taskRepositoryMock.Setup(x => x.GetByIdAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTask);
        _taskRepositoryMock.Setup(x => x.GetByTitleAsync(command.Title, It.IsAny<CancellationToken>()))
            .ReturnsAsync(null as TaskItem);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(existingTask.Id);
        result.Title.Should().Be(command.Title);
        result.Description.Should().Be(command.Description);
        result.DueDate.Should().Be(command.DueDate);

        _taskRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _publishEndpointMock.Verify(x => x.Publish(It.IsAny<TaskUpdatedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NonExistingTask_ShouldThrowTaskNotFoundException()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        _taskRepositoryMock.Setup(x => x.GetByIdAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(null as TaskItem);

        var command = new UpdateTaskCommand
        {
            Id = taskId,
            Title = "Updated Title",
            Description = "Updated Description",
            DueDate = DateTime.UtcNow.AddDays(1)
        };

        // Act & Assert
        await Assert.ThrowsAsync<TaskNotFoundException>(() => _handler.Handle(command, CancellationToken.None));
        _taskRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_DuplicateTitle_ShouldThrowDuplicateTaskTitleException()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var existingTask = new TaskItem("Old Title", "Old Description", DateTime.UtcNow.AddDays(2));
        var otherTask = new TaskItem("New Title", "Other Description", DateTime.UtcNow.AddDays(3));

        var command = new UpdateTaskCommand
        {
            Id = taskId,
            Title = "New Title",
            Description = "Updated Description",
            DueDate = DateTime.UtcNow.AddDays(1)
        };

        _taskRepositoryMock.Setup(x => x.GetByIdAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTask);
        _taskRepositoryMock.Setup(x => x.GetByTitleAsync(command.Title, It.IsAny<CancellationToken>()))
            .ReturnsAsync(otherTask);

        // Act & Assert
        await Assert.ThrowsAsync<DuplicateTaskTitleException>(() => _handler.Handle(command, CancellationToken.None));
        _taskRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(-1)] // Yesterday
    [InlineData(-7)] // A week ago
    [InlineData(-30)] // A month ago
    public async Task Handle_PastDueDate_ShouldFailValidation(int daysOffset)
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var command = new UpdateTaskCommand
        {
            Id = taskId,
            Title = "Updated Title",
            Description = "Updated Description",
            DueDate = DateTime.UtcNow.Date.AddDays(daysOffset)
        };

        // Act
        var validationResult = await _validator.ValidateAsync(command);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().Contain(e =>
            e.PropertyName == "DueDate" &&
            e.ErrorMessage.Contains("future date", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Handle_WhenPublishEndpointThrowsException_ShouldPropagateException()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var existingTask = new TaskItem("Old Title", "Old Description", DateTime.UtcNow.AddDays(2));

        var command = new UpdateTaskCommand
        {
            Id = taskId,
            Title = "Updated Title",
            Description = "Updated Description",
            DueDate = DateTime.UtcNow.AddDays(1)
        };

        _taskRepositoryMock.Setup(x => x.GetByIdAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTask);
        _taskRepositoryMock.Setup(x => x.GetByTitleAsync(command.Title, It.IsAny<CancellationToken>()))
            .ReturnsAsync(null as TaskItem);
        _publishEndpointMock.Setup(x => x.Publish(It.IsAny<TaskUpdatedEvent>(), It.IsAny<CancellationToken>()))
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
