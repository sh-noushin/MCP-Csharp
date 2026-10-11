using Microsoft.Extensions.Time.Testing;
using TaskTracker.Application.Tasks.Queries;
using TaskTracker.Application.Tasks.Services;
using TaskTracker.Domain.Projects;
using TaskTracker.Domain.Tasks;
using TaskTracker.TestSupport;

namespace TaskTracker.Application.Tests.Tasks.Services;

public sealed class ListTasksHandlerTests
{
    private readonly InMemoryProjectRepository _projects = new();
    private readonly FakeTimeProvider _timeProvider = TestClock.CreateProvider();
    private readonly Project _website = Project.Create("Website", TestClock.Now);
    private readonly Project _api = Project.Create("Api", TestClock.Now);
    private readonly ListTasksHandler _handler;

    public ListTasksHandlerTests()
    {
        _projects.Add(_website);
        _projects.Add(_api);
        _handler = new ListTasksHandler(_projects, _timeProvider);
    }

    [Fact]
    public async Task HandleAsync_OrdersByDueDateThenHighestPriority()
    {
        _website.AddTask("No due date", Priority.High, null, TestClock.Now);
        _website.AddTask("Later", Priority.High, TestClock.Today.AddDays(5), TestClock.Now);
        _api.AddTask("Soon low", Priority.Low, TestClock.Today.AddDays(1), TestClock.Now);
        _api.AddTask("Soon high", Priority.High, TestClock.Today.AddDays(1), TestClock.Now);

        var result = await _handler.Handle(new ListTasksQuery(), CancellationToken.None);

        Assert.Equal(["Soon high", "Soon low", "Later", "No due date"], result.Select(task => task.Title));
    }

    [Fact]
    public async Task HandleAsync_FiltersByProject()
    {
        _website.AddTask("Website task", Priority.Low, null, TestClock.Now);
        _api.AddTask("Api task", Priority.Low, null, TestClock.Now);

        var result = await _handler.Handle(new ListTasksQuery(ProjectId: _api.Id.Value), CancellationToken.None);

        Assert.Equal("Api task", Assert.Single(result).Title);
    }

    [Fact]
    public async Task HandleAsync_FiltersByStatus()
    {
        _website.AddTask("Waiting", Priority.Low, null, TestClock.Now);
        var started = _website.AddTask("Started", Priority.Low, null, TestClock.Now);
        _website.StartTask(started.Id);

        var result = await _handler.Handle(
            new ListTasksQuery(Status: TaskItemStatus.InProgress),
            CancellationToken.None);

        Assert.Equal("Started", Assert.Single(result).Title);
    }

    [Fact]
    public async Task HandleAsync_ReturnsOnlyOverdueTasksWhenAsked()
    {
        _website.AddTask("Missed", Priority.Low, TestClock.Today, TestClock.Now);
        var finished = _website.AddTask("Finished in time", Priority.Low, TestClock.Today, TestClock.Now);
        _website.CompleteTask(finished.Id, TestClock.Now);
        _website.AddTask("Still ahead", Priority.Low, TestClock.Today.AddDays(10), TestClock.Now);
        _timeProvider.Advance(TimeSpan.FromDays(2));

        var result = await _handler.Handle(new ListTasksQuery(OverdueOnly: true), CancellationToken.None);

        var overdue = Assert.Single(result);
        Assert.Equal("Missed", overdue.Title);
        Assert.True(overdue.IsOverdue);
    }

    [Fact]
    public async Task HandleAsync_ReturnsNothingForUnknownProject()
    {
        _website.AddTask("Website task", Priority.Low, null, TestClock.Now);

        var result = await _handler.Handle(new ListTasksQuery(ProjectId: Guid.NewGuid()), CancellationToken.None);

        Assert.Empty(result);
    }
}
