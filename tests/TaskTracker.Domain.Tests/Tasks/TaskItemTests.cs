using TaskTracker.Domain.Projects;
using TaskTracker.Domain.Tasks;

namespace TaskTracker.Domain.Tests.Tasks;

public sealed class TaskItemTests
{
    private readonly Project _project = Project.Create("Website", TestClock.Now);

    [Fact]
    public void IsOverdue_IsTrueForUnfinishedTaskPastItsDueDate()
    {
        var task = _project.AddTask("Ship", Priority.Low, TestClock.Today, TestClock.Now);

        Assert.True(task.IsOverdue(TestClock.Today.AddDays(1)));
    }

    [Fact]
    public void IsOverdue_IsFalseOnTheDueDate()
    {
        var task = _project.AddTask("Ship", Priority.Low, TestClock.Today, TestClock.Now);

        Assert.False(task.IsOverdue(TestClock.Today));
    }

    [Fact]
    public void IsOverdue_IsFalseForFinishedTask()
    {
        var task = _project.AddTask("Ship", Priority.Low, TestClock.Today, TestClock.Now);
        _project.CompleteTask(task.Id, TestClock.Now);

        Assert.False(task.IsOverdue(TestClock.Today.AddDays(1)));
    }

    [Fact]
    public void IsOverdue_IsFalseWithoutDueDate()
    {
        var task = _project.AddTask("Ship", Priority.Low, null, TestClock.Now);

        Assert.False(task.IsOverdue(TestClock.Today.AddDays(1)));
    }

    [Fact]
    public void TaskItemId_IsUniqueAndPrintsItsValue()
    {
        var id = TaskItemId.New();

        Assert.NotEqual(id, TaskItemId.New());
        Assert.Equal(id.Value.ToString(), id.ToString());
    }

    [Fact]
    public void TaskTitle_ComparesByValueAndPrintsItsValue()
    {
        var title = TaskTitle.Create("Ship");

        Assert.Equal(TaskTitle.Create(" Ship "), title);
        Assert.Equal("Ship", title.ToString());
    }
}
