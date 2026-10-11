using Microsoft.EntityFrameworkCore;
using TaskTracker.Domain.Projects;
using TaskTracker.Domain.Tasks;
using TaskTracker.TestSupport;

namespace TaskTracker.Infrastructure.Tests.Persistence;

public sealed class TaskTrackerDbContextTests : IDisposable
{
    private readonly SqliteTestDatabase _database = new();

    public void Dispose() => _database.Dispose();

    [Fact]
    public void Model_MapsEachEntityToItsOwnTable()
    {
        using var context = _database.CreateContext();

        Assert.Equal("Projects", context.Model.FindEntityType(typeof(Project))?.GetTableName());
        Assert.Equal("Tasks", context.Model.FindEntityType(typeof(TaskItem))?.GetTableName());
    }

    [Fact]
    public async Task SaveChanges_StoresProjectAndReadsItBackUnchanged()
    {
        var project = Project.Create("Website", TestClock.Now);
        await SaveAsync(project);

        var loaded = await LoadAsync(project.Id);

        Assert.Equal(project.Id, loaded.Id);
        Assert.Equal(project.Name, loaded.Name);
        Assert.Equal(ProjectStatus.Active, loaded.Status);
        Assert.Equal(TestClock.Now, loaded.CreatedAt);
        Assert.Empty(loaded.Tasks);
    }

    [Fact]
    public async Task SaveChanges_StoresTasksWithTheirProject()
    {
        var project = Project.Create("Website", TestClock.Now);
        var dueDate = TestClock.Today.AddDays(3);
        var task = project.AddTask("Write README", Priority.High, dueDate, TestClock.Now);
        await SaveAsync(project);

        var loaded = Assert.Single((await LoadAsync(project.Id)).Tasks);

        Assert.Equal(task.Id, loaded.Id);
        Assert.Equal(project.Id, loaded.ProjectId);
        Assert.Equal(task.Title, loaded.Title);
        Assert.Equal(Priority.High, loaded.Priority);
        Assert.Equal(TaskItemStatus.Todo, loaded.Status);
        Assert.Equal(dueDate, loaded.DueDate);
        Assert.Equal(TestClock.Now, loaded.CreatedAt);
        Assert.Null(loaded.CompletedAt);
    }

    [Fact]
    public async Task SaveChanges_StoresChangesMadeToALoadedProject()
    {
        var project = Project.Create("Website", TestClock.Now);
        var first = project.AddTask("First", Priority.Low, null, TestClock.Now);
        await SaveAsync(project);
        var completedAt = TestClock.Now.AddHours(1);

        await using (var context = _database.CreateContext())
        {
            var tracked = await context.Projects.Include(p => p.Tasks).SingleAsync();
            tracked.CompleteTask(first.Id, completedAt);
            tracked.AddTask("Second", Priority.Medium, null, TestClock.Now);
            tracked.Archive();
            await context.SaveChangesAsync(CancellationToken.None);
        }

        var loaded = await LoadAsync(project.Id);
        Assert.Equal(ProjectStatus.Archived, loaded.Status);
        Assert.Equal(2, loaded.Tasks.Count);
        var completed = loaded.Tasks.Single(task => task.Id == first.Id);
        Assert.Equal(TaskItemStatus.Done, completed.Status);
        Assert.Equal(completedAt, completed.CompletedAt);
    }

    [Fact]
    public async Task Database_StoresEnumsAsReadableText()
    {
        var project = Project.Create("Website", TestClock.Now);
        project.AddTask("Write README", Priority.High, null, TestClock.Now);
        await SaveAsync(project);

        await using var context = _database.CreateContext();
        var stored = await context.Database
            .SqlQuery<string>($"SELECT Priority || '/' || Status AS Value FROM Tasks")
            .SingleAsync();

        Assert.Equal("High/Todo", stored);
    }

    private async Task SaveAsync(Project project)
    {
        await using var context = _database.CreateContext();
        context.Projects.Add(project);
        await context.SaveChangesAsync(CancellationToken.None);
    }

    private async Task<Project> LoadAsync(ProjectId id)
    {
        await using var context = _database.CreateContext();

        return await context.Projects.Include(project => project.Tasks).SingleAsync(project => project.Id == id);
    }
}
