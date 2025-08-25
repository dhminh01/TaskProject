using Moq;
using TaskService.Application.Tasks.Queries.GetTaskDetail;
using TaskService.Domain.Entities;
using TaskService.Domain.Interfaces;
using FluentAssertions;
using TaskService.Application.Tasks.Queries;

namespace TaskService.FunctionalTests.Handlers;

public class GetTaskDetailHandlerTest
{
    private readonly Mock<ITaskRepository> _taskRepositoryMock;
    private readonly GetTaskDetailHandler _handler;

    public GetTaskDetailHandlerTest()
    {
        _taskRepositoryMock = new Mock<ITaskRepository>();
        _handler = new GetTaskDetailHandler(_taskRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ExistingTask_ShouldReturnTaskDetail()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var taskItem = new TaskItem(
            "Test Task",
            "Test Description",
            DateTime.UtcNow.AddDays(1));

        _taskRepositoryMock.Setup(x => x.GetByIdAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(taskItem);

        var query = new GetTaskDetailQuery(taskId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Title.Should().Be(taskItem.Title);
        result.Description.Should().Be(taskItem.Description);
        result.DueDate.Should().Be(taskItem.DueDate);
        _taskRepositoryMock.Verify(x => x.GetByIdAsync(taskId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NonExistingTask_ShouldReturnNull()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        _taskRepositoryMock.Setup(x => x.GetByIdAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(null as TaskItem);

        var query = new GetTaskDetailQuery(taskId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
        _taskRepositoryMock.Verify(x => x.GetByIdAsync(taskId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
