using TaskTracker.Domain.Common;
using TaskTracker.Domain.Projects;
using TaskTracker.Domain.Tasks;

namespace TaskTracker.Domain.Tests.Projects;

public sealed class ProjectTaskTests
{
    private readonly Project _project = Project.Create("Website", TestClock.Now);

    [Fact]
    public void AddTask_AddsTodoTaskToProject()
    {
        var dueDate = TestClock.Today.AddDays(3);

        var task = _project.AddTask("  Write README ", Priority.High, dueDate, TestClock.Now);

        Assert.Same(task, Assert.Single(_project.Tasks));
        Assert.Equal(_project.Id, task.ProjectId);
        Assert.Equal("Write README", task.Title.Value);
        Assert.Equal(Priority.High, task.Priority);
        Assert.Equal(dueDate, task.DueDate);
        Assert.Equal(TaskItemStatus.Todo, task.Status);
        Assert.Equal(TestClock.Now, task.CreatedAt);
        Assert.Null(task.CompletedAt);
    }

    [Fact]
    public void AddTask_AcceptsDueDateOfToday()
    {
        var task = _project.AddTask("Ship", Priority.Low, TestClock.Today, TestClock.Now);

        Assert.Equal(TestClock.Today, task.DueDate);
    }

    [Fact]
    public void AddTask_RejectsDueDateInThePast()
    {
        var yesterday = TestClock.Today.AddDays(-1);

        var exception = Assert.Throws<DomainException>(
            () => _project.AddTask("Ship", Priority.Low, yesterday, TestClock.Now));

        Assert.Equal("Due date cannot be in the past.", exception.Message);
        Assert.Empty(_project.Tasks);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(" ")]
    public void AddTask_RejectsMissingTitle(string? title)
    {
        Assert.Throws<DomainException>(() => _project.AddTask(title, Priority.Low, null, TestClock.Now));
    }

    [Fact]
    public void AddTask_RejectsTitleLongerThanMaximum()
    {
        var tooLong = new string('a', TaskTitle.MaxLength + 1);

        Assert.Throws<DomainException>(() => _project.AddTask(tooLong, Priority.Low, null, TestClock.Now));
    }

    [Fact]
    public void AddTask_RejectsUndefinedPriority()
    {
        Assert.Throws<DomainException>(() => _project.AddTask("Ship", (Priority)42, null, TestClock.Now));
    }

    [Fact]
    public void AddTask_RejectsArchivedProject()
    {
        _project.Archive();

        Assert.Throws<DomainException>(() => _project.AddTask("Ship", Priority.Low, null, TestClock.Now));
    }

    [Fact]
    public void StartTask_MovesTodoTaskToInProgress()
    {
        var task = _project.AddTask("Ship", Priority.Low, null, TestClock.Now);

        var started = _project.StartTask(task.Id);

        Assert.Same(task, started);
        Assert.Equal(TaskItemStatus.InProgress, task.Status);
    }

    [Fact]
    public void StartTask_RejectsTaskThatIsNotTodo()
    {
        var task = _project.AddTask("Ship", Priority.Low, null, TestClock.Now);
        _project.StartTask(task.Id);

        Assert.Throws<DomainException>(() => _project.StartTask(task.Id));
    }

    [Fact]
    public void StartTask_RejectsTaskOfAnotherProject()
    {
        Assert.Throws<DomainException>(() => _project.StartTask(TaskItemId.New()));
    }

    [Fact]
    public void StartTask_RejectsArchivedProject()
    {
        var task = _project.AddTask("Ship", Priority.Low, null, TestClock.Now);
        _project.Archive();

        Assert.Throws<DomainException>(() => _project.StartTask(task.Id));
    }

    [Fact]
    public void CompleteTask_MarksTaskDoneAndRecordsWhen()
    {
        var task = _project.AddTask("Ship", Priority.Low, null, TestClock.Now);
        var completedAt = TestClock.Now.AddHours(2);

        _project.CompleteTask(task.Id, completedAt);

        Assert.Equal(TaskItemStatus.Done, task.Status);
        Assert.Equal(completedAt, task.CompletedAt);
    }

    [Fact]
    public void CompleteTask_RejectsTaskThatIsAlreadyDone()
    {
        var task = _project.AddTask("Ship", Priority.Low, null, TestClock.Now);
        _project.CompleteTask(task.Id, TestClock.Now);

        Assert.Throws<DomainException>(() => _project.CompleteTask(task.Id, TestClock.Now));
    }

    [Fact]
    public void CompleteTask_RejectsArchivedProject()
    {
        var task = _project.AddTask("Ship", Priority.Low, null, TestClock.Now);
        _project.Archive();

        Assert.Throws<DomainException>(() => _project.CompleteTask(task.Id, TestClock.Now));
    }
}
