using ModelContextProtocol;
using TaskTracker.McpServer.Tools;
using TaskTracker.TestSupport;

namespace TaskTracker.McpServer.Tests.Tools;

public sealed class TaskToolsTests : IAsyncLifetime
{
    private readonly ToolTestContext _context = new();
    private readonly TaskTools _tools;
    private Guid _projectId;

    public TaskToolsTests() => _tools = new TaskTools(_context.Sender);

    public async Task InitializeAsync() =>
        _projectId = (await new ProjectTools(_context.Sender).CreateProjectAsync("Website", CancellationToken.None)).Id;

    public Task DisposeAsync()
    {
        _context.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task AddTask_UsesMediumPriorityAndNoDueDateByDefault()
    {
        var task = await _tools.AddTaskAsync(_projectId, "Write README");

        Assert.Equal(_projectId, task.ProjectId);
        Assert.Equal("Write README", task.Title);
        Assert.Equal("Medium", task.Priority);
        Assert.Equal("Todo", task.Status);
        Assert.Null(task.DueDate);
    }

    [Theory]
    [InlineData("High")]
    [InlineData("high")]
    [InlineData(" HIGH ")]
    public async Task AddTask_AcceptsPriorityInAnyCasing(string priority)
    {
        var dueDate = TestClock.Today.AddDays(1);

        var task = await _tools.AddTaskAsync(_projectId, "Ship", priority, dueDate);

        Assert.Equal("High", task.Priority);
        Assert.Equal(dueDate, task.DueDate);
    }

    [Theory]
    [InlineData("urgent")]
    [InlineData("7")]
    public async Task AddTask_ExplainsWhichPrioritiesAreAllowed(string priority)
    {
        var exception = await Assert.ThrowsAsync<McpException>(
            () => _tools.AddTaskAsync(_projectId, "Ship", priority));

        Assert.Equal($"'{priority}' is not a valid priority. Use one of: Low, Medium, High.", exception.Message);
    }

    [Fact]
    public async Task AddTask_ReportsABusinessRuleFailureToTheClient()
    {
        var exception = await Assert.ThrowsAsync<McpException>(
            () => _tools.AddTaskAsync(_projectId, "Late", dueDate: TestClock.Today.AddDays(-1)));

        Assert.Equal("Due date cannot be in the past.", exception.Message);
    }

    [Fact]
    public async Task StartTask_MovesTheTaskToInProgress()
    {
        var task = await _tools.AddTaskAsync(_projectId, "Ship");

        var started = await _tools.StartTaskAsync(task.Id, CancellationToken.None);

        Assert.Equal("InProgress", started.Status);
    }

    [Fact]
    public async Task CompleteTask_MarksTheTaskDone()
    {
        var task = await _tools.AddTaskAsync(_projectId, "Ship");

        var completed = await _tools.CompleteTaskAsync(task.Id, CancellationToken.None);

        Assert.Equal("Done", completed.Status);
        Assert.Equal(TestClock.Now, completed.CompletedAt);
    }

    [Fact]
    public async Task CompleteTask_ReportsAnUnknownTaskToTheClient()
    {
        var unknownId = Guid.NewGuid();

        var exception = await Assert.ThrowsAsync<McpException>(
            () => _tools.CompleteTaskAsync(unknownId, CancellationToken.None));

        Assert.Equal($"Task '{unknownId}' was not found.", exception.Message);
    }

    [Theory]
    [InlineData("InProgress")]
    [InlineData("in_progress")]
    [InlineData("in progress")]
    public async Task ListTasks_FiltersByStatusWrittenInDifferentStyles(string status)
    {
        await _tools.AddTaskAsync(_projectId, "Waiting");
        var started = await _tools.AddTaskAsync(_projectId, "Started");
        await _tools.StartTaskAsync(started.Id, CancellationToken.None);

        var listed = await _tools.ListTasksAsync(status: status);

        Assert.Equal("Started", Assert.Single(listed).Title);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(" ")]
    public async Task ListTasks_ReturnsEveryTaskWithoutAStatusFilter(string? status)
    {
        await _tools.AddTaskAsync(_projectId, "First");
        await _tools.AddTaskAsync(_projectId, "Second");

        var listed = await _tools.ListTasksAsync(_projectId, status);

        Assert.Equal(2, listed.Count);
    }

    [Fact]
    public async Task ListTasks_ExplainsWhichStatusesAreAllowed()
    {
        var exception = await Assert.ThrowsAsync<McpException>(() => _tools.ListTasksAsync(status: "blocked"));

        Assert.Equal("'blocked' is not a valid status. Use one of: Todo, InProgress, Done.", exception.Message);
    }

    [Fact]
    public async Task ListTasks_ReturnsOnlyOverdueTasksWhenAsked()
    {
        await _tools.AddTaskAsync(_projectId, "Missed", dueDate: TestClock.Today);
        await _tools.AddTaskAsync(_projectId, "Still ahead", dueDate: TestClock.Today.AddDays(10));
        _context.TimeProvider.Advance(TimeSpan.FromDays(2));

        var listed = await _tools.ListTasksAsync(overdueOnly: true);

        Assert.Equal("Missed", Assert.Single(listed).Title);
    }
}
