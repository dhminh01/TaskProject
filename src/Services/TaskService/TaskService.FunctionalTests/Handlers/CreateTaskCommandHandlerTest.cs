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

[Trait("Category", "Unit")]
public class CreateTaskCommandHandlerTest
{
    private readonly Mock<ITaskRepository> _taskRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IPublishEndpoint> _publishEndpointMock;
    private readonly Mock<ILogger<CreateTaskCommandHandler>> _loggerMock;
    private readonly CreateTaskCommandHandler _handler;
    private readonly IValidator<CreateTaskCommand> _validator;

    private static readonly DateTime CurrentUtcDate = DateTime.UtcNow.Date;

    public static IEnumerable<object[]> ValidTaskCommands =>
        new List<object[]>
        {
            new object[] { "Task 1", "Description 1", CurrentUtcDate.AddDays(1) },
            new object[] { "Task 2", "Description 2", CurrentUtcDate.AddMonths(1) },
            new object[] { "Task 3", "Description 3", CurrentUtcDate.AddYears(1) }
        };

    public static IEnumerable<object[]> InvalidDueDates =>
        new List<object[]>
        {
            new object[] { CurrentUtcDate.AddDays(-1), "past date" },
            new object[] { CurrentUtcDate.AddYears(-1), "past date" },
            new object[] { CurrentUtcDate, "today's date" }
        };

    public CreateTaskCommandHandlerTest()
    {
        _taskRepositoryMock = new Mock<ITaskRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _publishEndpointMock = new Mock<IPublishEndpoint>();
        _loggerMock = new Mock<ILogger<CreateTaskCommandHandler>>();
        _validator = new CreateTaskCommandValidator(_taskRepositoryMock.Object);
        _handler = new CreateTaskCommandHandler(
            _taskRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _publishEndpointMock.Object,
            _loggerMock.Object);
    }

    [Theory]
    [MemberData(nameof(ValidTaskCommands))]
    [Trait("Category", "Handler")]
    public async Task Handle_ValidCommand_ShouldCreateTaskAndPublishEvent(string title, string description, DateTime dueDate)
    {
        // Arrange
        var command = new CreateTaskCommand
        {
            Title = title,
            Description = description,
            DueDate = dueDate
        };

        _taskRepositoryMock.Setup(x => x.GetByTitleAsync(command.Title, It.IsAny<CancellationToken>()))
            .ReturnsAsync(null as TaskItem);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(command, opts =>
            opts.ExcludingMissingMembers());

        _taskRepositoryMock.Verify(x => x.AddAsync(It.Is<TaskItem>(t =>
            t.Title == command.Title &&
            t.Description == command.Description &&
            t.DueDate == command.DueDate),
            It.IsAny<CancellationToken>()), Times.Once);

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        _publishEndpointMock.Verify(x => x.Publish(It.Is<TaskCreatedEvent>(e =>
            e.Title == command.Title &&
            e.Description == command.Description),
            It.IsAny<CancellationToken>()), Times.Once);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(command.Title)),
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
    [MemberData(nameof(InvalidDueDates))]
    [Trait("Category", "Validation")]
    public async Task Handle_InvalidDueDate_ShouldFailValidation(DateTime dueDate, string reason)
    {
        // Arrange
        var command = new CreateTaskCommand
        {
            Title = "Test Task",
            Description = "Test Description",
            DueDate = dueDate
        };

        // Act
        var validationResult = await _validator.ValidateAsync(command);

        // Assert
        validationResult.IsValid.Should().BeFalse($"because {reason} should not be allowed");
        validationResult.Errors.Should().Contain(e =>
            e.PropertyName == nameof(CreateTaskCommand.DueDate) &&
            e.ErrorMessage.Contains("future date", StringComparison.OrdinalIgnoreCase));
    }

    [Theory]
    [InlineData("", "Description", "Title is required")]
    [InlineData("   ", "Description", "Title cannot be empty")]
    [InlineData("Title", "", "Description is required")]
    [InlineData("Title", "   ", "Description cannot be empty")]
    [Trait("Category", "Validation")]
    public async Task Handle_InvalidCommand_ShouldFailValidation(string title, string description, string expectedError)
    {
        // Arrange
        var command = new CreateTaskCommand
        {
            Title = title,
            Description = description,
            DueDate = DateTime.UtcNow.AddDays(1)
        };

        // Act
        var validationResult = await _validator.ValidateAsync(command);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().Contain(e =>
            e.ErrorMessage.Contains(expectedError, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    [Trait("Category", "ErrorHandling")]
    public async Task Handle_WhenRepositoryThrowsException_ShouldPropagateException()
    {
        // Arrange
        var command = new CreateTaskCommand
        {
            Title = "Test Task",
            Description = "Test Description",
            DueDate = DateTime.UtcNow.AddDays(1)
        };

        var dbException = new Exception("Database error");
        _taskRepositoryMock.Setup(x => x.GetByTitleAsync(command.Title, It.IsAny<CancellationToken>()))
            .ReturnsAsync(null as TaskItem);
        _taskRepositoryMock.Setup(x => x.AddAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(dbException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() =>
            _handler.Handle(command, CancellationToken.None));

        exception.Should().Be(dbException);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Database error")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    [Trait("Category", "ErrorHandling")]
    public async Task Handle_WhenPublishEndpointThrowsException_ShouldPropagateException()
    {
        // Arrange
        var command = new CreateTaskCommand
        {
            Title = "Test Task",
            Description = "Test Description",
            DueDate = DateTime.UtcNow.AddDays(1)
        };

        var publisherException = new Exception("Publisher error");
        _taskRepositoryMock.Setup(x => x.GetByTitleAsync(command.Title, It.IsAny<CancellationToken>()))
            .ReturnsAsync(null as TaskItem);
        _publishEndpointMock.Setup(x => x.Publish(It.IsAny<TaskCreatedEvent>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(publisherException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() =>
            _handler.Handle(command, CancellationToken.None));

        exception.Should().Be(publisherException);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Publisher error")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);

        // Verify that changes were rolled back
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}