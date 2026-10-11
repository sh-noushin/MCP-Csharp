using TaskTracker.Application.Common;
using TaskTracker.Application.Tasks.Commands;
using TaskTracker.Application.Tasks.Services;
using TaskTracker.Domain.Common;
using TaskTracker.Domain.Projects;
using TaskTracker.Domain.Tasks;
using TaskTracker.TestSupport;

namespace TaskTracker.Application.Tests.Tasks.Services;

public sealed class AddTaskHandlerTests
{
    private readonly InMemoryProjectRepository _projects = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly Project _project = Project.Create("Website", TestClock.Now);
    private readonly AddTaskHandler _handler;

    public AddTaskHandlerTests()
    {
        _projects.Add(_project);
        _handler = new AddTaskHandler(_projects, _unitOfWork, TestClock.CreateProvider());
    }

    [Fact]
    public async Task HandleAsync_AddsTaskToProject()
    {
        var dueDate = TestClock.Today.AddDays(2);
        var command = new AddTaskCommand(_project.Id.Value, "Write README", Priority.High, dueDate);

        var result = await _handler.Handle(command, CancellationToken.None);

        var stored = Assert.Single(_project.Tasks);
        Assert.Equal(stored.Id.Value, result.Id);
        Assert.Equal(_project.Id.Value, result.ProjectId);
        Assert.Equal("Write README", result.Title);
        Assert.Equal("High", result.Priority);
        Assert.Equal("Todo", result.Status);
        Assert.Equal(dueDate, result.DueDate);
        Assert.False(result.IsOverdue);
        Assert.Equal(TestClock.Now, result.CreatedAt);
        Assert.Null(result.CompletedAt);
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task HandleAsync_ReportsUnknownProject()
    {
        var command = new AddTaskCommand(Guid.NewGuid(), "Write README", Priority.Low, null);

        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));

        Assert.Equal(0, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task HandleAsync_DoesNotSaveWhenDomainRejectsTask()
    {
        var command = new AddTaskCommand(_project.Id.Value, "Late", Priority.Low, TestClock.Today.AddDays(-1));

        await Assert.ThrowsAsync<DomainException>(() => _handler.Handle(command, CancellationToken.None));

        Assert.Equal(0, _unitOfWork.SaveCount);
    }
}
