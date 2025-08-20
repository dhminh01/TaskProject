using Moq;
using TaskService.Application.Tasks.Commands.CreateTask;
using TaskService.Domain.Entities;
using TaskService.Domain.Events;
using TaskService.Domain.Interfaces;
using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging;
using TaskService.Application.Tasks.Commands;

namespace TaskService.FunctionalTests.Handlers;

public class CreateTaskCommandHandlerTest
{
    private readonly Mock<ITaskRepository> _taskRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IPublishEndpoint> _publishEndpointMock;
    private readonly Mock<ILogger<CreateTaskCommandHandler>> _loggerMock;
    private readonly CreateTaskCommandHandler _handler;

    public CreateTaskCommandHandlerTest()
    {
        _taskRepositoryMock = new Mock<ITaskRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _publishEndpointMock = new Mock<IPublishEndpoint>();
        _loggerMock = new Mock<ILogger<CreateTaskCommandHandler>>();
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
        var command = new CreateTaskCommand(
            "Test Task",
            "Test Description",
            DateTime.UtcNow.AddDays(1)
        );

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
    public async Task Handle_WhenRepositoryThrowsException_ShouldPropagateException()
    {
        // Arrange
        var command = new CreateTaskCommand(
            "Test Task",
            "Test Description",
            DateTime.UtcNow.AddDays(1)
        );

        _taskRepositoryMock.Setup(x => x.AddAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenPublishEndpointThrowsException_ShouldPropagateException()
    {
        // Arrange
        var command = new CreateTaskCommand(
            "Test Task",
            "Test Description",
            DateTime.UtcNow.AddDays(1)
        );

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