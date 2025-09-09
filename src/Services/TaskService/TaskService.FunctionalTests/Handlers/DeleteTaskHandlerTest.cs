using Moq;
using TaskService.Application.Tasks.Commands;
using TaskService.Domain.Entities;
using TaskService.Domain.Interfaces;
using FluentAssertions;
using MassTransit;
using TaskService.Domain.Events;

namespace TaskService.FunctionalTests.Handlers;

public class DeleteTaskHandlerTest
{
    private readonly Mock<ITaskRepository> _taskRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IPublishEndpoint> _publishEndpointMock;
    private readonly DeleteTaskHandler _handler;

    public DeleteTaskHandlerTest()
    {
        _taskRepositoryMock = new Mock<ITaskRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _publishEndpointMock = new Mock<IPublishEndpoint>();
        _handler = new DeleteTaskHandler(_unitOfWorkMock.Object, _taskRepositoryMock.Object, _publishEndpointMock.Object);
    }

    [Fact]
    public async Task Handle_ExistingTask_ShouldDeleteAndReturnTrue()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var taskItem = new TaskItem("Test Task", "Test Description", DateTime.UtcNow.AddDays(1));

        _taskRepositoryMock.Setup(x => x.GetByIdAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(taskItem);
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1); // Indicate one row was affected

        var command = new DeleteTaskCommand(taskId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        _taskRepositoryMock.Verify(x => x.GetByIdAsync(taskId, It.IsAny<CancellationToken>()), Times.Once);
        _taskRepositoryMock.Verify(x => x.DeleteAsync(taskItem, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _publishEndpointMock.Verify(x => x.Publish(
            It.Is<TaskDeletedEvent>(e =>
                e.Id == taskItem.Id &&
                e.Title == taskItem.Title &&
                e.Description == taskItem.Description &&
                e.DueDate == taskItem.DueDate),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_NonExistingTask_ShouldReturnFalse()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        _taskRepositoryMock.Setup(x => x.GetByIdAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(null as TaskItem);

        var command = new DeleteTaskCommand(taskId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
        _taskRepositoryMock.Verify(x => x.GetByIdAsync(taskId, It.IsAny<CancellationToken>()), Times.Once);
        _taskRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrowsException_ShouldPropagateException()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var taskItem = new TaskItem("Test Task", "Test Description", DateTime.UtcNow.AddDays(1));

        _taskRepositoryMock.Setup(x => x.GetByIdAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(taskItem);
        _taskRepositoryMock.Setup(x => x.DeleteAsync(taskItem, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        var command = new DeleteTaskCommand(taskId);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
    }
}
