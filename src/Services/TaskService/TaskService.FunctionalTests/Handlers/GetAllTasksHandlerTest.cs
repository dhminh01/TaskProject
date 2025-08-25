using Moq;
using TaskService.Application.Tasks.Queries;
using TaskService.Domain.Entities;
using TaskService.Domain.Interfaces;
using FluentAssertions;

namespace TaskService.FunctionalTests.Handlers;

public class GetAllTasksHandlerTest
{
    private readonly Mock<ITaskRepository> _taskRepositoryMock;
    private readonly GetAllTasksHandler _handler;

    public GetAllTasksHandlerTest()
    {
        _taskRepositoryMock = new Mock<ITaskRepository>();
        _handler = new GetAllTasksHandler(_taskRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnAllTasks()
    {
        // Arrange
        var tasks = new List<TaskItem>
        {
            new TaskItem("Task 1", "Description 1", DateTime.UtcNow.AddDays(1)),
            new TaskItem("Task 2", "Description 2", DateTime.UtcNow.AddDays(2)),
            new TaskItem("Task 3", "Description 3", DateTime.UtcNow.AddDays(3))
        };

        _taskRepositoryMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(tasks);

        var query = new GetAllTasksQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Select(t => t.Title).Should().BeEquivalentTo(tasks.Select(t => t.Title));
        result.Select(t => t.Description).Should().BeEquivalentTo(tasks.Select(t => t.Description));
        _taskRepositoryMock.Verify(x => x.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NoTasks_ShouldReturnEmptyList()
    {
        // Arrange
        _taskRepositoryMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TaskItem>());

        var query = new GetAllTasksQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
        _taskRepositoryMock.Verify(x => x.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
