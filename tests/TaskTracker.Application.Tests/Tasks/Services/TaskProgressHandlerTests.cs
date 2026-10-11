using TaskTracker.Application.Common;
using TaskTracker.Application.Tasks.Commands;
using TaskTracker.Application.Tasks.Services;
using TaskTracker.Domain.Common;
using TaskTracker.Domain.Projects;
using TaskTracker.Domain.Tasks;
using TaskTracker.TestSupport;

namespace TaskTracker.Application.Tests.Tasks.Services;

public sealed class TaskProgressHandlerTests
{
    private readonly InMemoryProjectRepository _projects = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly StartTaskHandler _startTask;
    private readonly CompleteTaskHandler _completeTask;
    private readonly TaskItem _task;

    public TaskProgressHandlerTests()
    {
        var project = Project.Create("Website", TestClock.Now);
        _task = project.AddTask("Ship", Priority.Medium, null, TestClock.Now);
        _projects.Add(project);

        var timeProvider = TestClock.CreateProvider();
        _startTask = new StartTaskHandler(_projects, _unitOfWork, timeProvider);
        _completeTask = new CompleteTaskHandler(_projects, _unitOfWork, timeProvider);
    }

    [Fact]
    public async Task StartTask_MovesTaskToInProgress()
    {
        var result = await _startTask.Handle(new StartTaskCommand(_task.Id.Value), CancellationToken.None);

        Assert.Equal("InProgress", result.Status);
        Assert.Equal(TaskItemStatus.InProgress, _task.Status);
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task StartTask_ReportsUnknownTask()
    {
        await Assert.ThrowsAsync<NotFoundException>(
            () => _startTask.Handle(new StartTaskCommand(Guid.NewGuid()), CancellationToken.None));

        Assert.Equal(0, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task CompleteTask_MarksTaskDone()
    {
        var result = await _completeTask.Handle(new CompleteTaskCommand(_task.Id.Value), CancellationToken.None);

        Assert.Equal("Done", result.Status);
        Assert.Equal(TestClock.Now, result.CompletedAt);
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task CompleteTask_ReportsUnknownTask()
    {
        await Assert.ThrowsAsync<NotFoundException>(
            () => _completeTask.Handle(new CompleteTaskCommand(Guid.NewGuid()), CancellationToken.None));
    }

    [Fact]
    public async Task CompleteTask_DoesNotSaveWhenTaskIsAlreadyDone()
    {
        var command = new CompleteTaskCommand(_task.Id.Value);
        await _completeTask.Handle(command, CancellationToken.None);

        await Assert.ThrowsAsync<DomainException>(() => _completeTask.Handle(command, CancellationToken.None));

        Assert.Equal(1, _unitOfWork.SaveCount);
    }
}
