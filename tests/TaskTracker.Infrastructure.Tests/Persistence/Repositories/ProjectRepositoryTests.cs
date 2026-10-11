using TaskTracker.Domain.Projects;
using TaskTracker.Domain.Tasks;
using TaskTracker.Infrastructure.Persistence.Repositories;
using TaskTracker.TestSupport;

namespace TaskTracker.Infrastructure.Tests.Persistence.Repositories;

public sealed class ProjectRepositoryTests : IDisposable
{
    private readonly SqliteTestDatabase _database = new();

    public void Dispose() => _database.Dispose();

    [Fact]
    public async Task Add_StoresProjectOnceChangesAreSaved()
    {
        var project = Project.Create("Website", TestClock.Now);

        await using (var context = _database.CreateContext())
        {
            new ProjectRepository(context).Add(project);
            await context.SaveChangesAsync(CancellationToken.None);
        }

        Assert.NotNull(await FindAsync(project.Id));
    }

    [Fact]
    public async Task Add_StoresNothingUntilChangesAreSaved()
    {
        var project = Project.Create("Website", TestClock.Now);

        await using (var context = _database.CreateContext())
        {
            new ProjectRepository(context).Add(project);
        }

        Assert.Null(await FindAsync(project.Id));
    }

    [Fact]
    public async Task FindAsync_ReturnsProjectWithItsTasks()
    {
        var project = await SeedAsync("Website", "Write README", "Ship");

        var found = await FindAsync(project.Id);

        Assert.NotNull(found);
        Assert.Equal("Website", found.Name.Value);
        Assert.Equal(["Ship", "Write README"], found.Tasks.Select(task => task.Title.Value).Order());
    }

    [Fact]
    public async Task FindAsync_ReturnsNullForUnknownProject()
    {
        await SeedAsync("Website");

        Assert.Null(await FindAsync(ProjectId.New()));
    }

    [Fact]
    public async Task FindByTaskAsync_ReturnsTheProjectThatOwnsTheTask()
    {
        await SeedAsync("Other", "Unrelated");
        var owner = await SeedAsync("Website", "Write README", "Ship");
        var taskId = owner.Tasks.First().Id;

        await using var context = _database.CreateContext();
        var found = await new ProjectRepository(context).FindByTaskAsync(taskId, CancellationToken.None);

        Assert.NotNull(found);
        Assert.Equal(owner.Id, found.Id);
        Assert.Equal(2, found.Tasks.Count);
    }

    [Fact]
    public async Task FindByTaskAsync_ReturnsNullForUnknownTask()
    {
        await SeedAsync("Website", "Write README");

        await using var context = _database.CreateContext();
        var found = await new ProjectRepository(context).FindByTaskAsync(TaskItemId.New(), CancellationToken.None);

        Assert.Null(found);
    }

    [Fact]
    public async Task ListAsync_ReturnsEveryProjectWithItsTasks()
    {
        await SeedAsync("Website", "Write README");
        await SeedAsync("Api");

        await using var context = _database.CreateContext();
        var projects = await new ProjectRepository(context).ListAsync(CancellationToken.None);

        Assert.Equal(["Api", "Website"], projects.Select(project => project.Name.Value).Order());
        Assert.Single(projects.Single(project => project.Name.Value == "Website").Tasks);
    }

    private async Task<Project> SeedAsync(string name, params string[] taskTitles)
    {
        var project = Project.Create(name, TestClock.Now);
        foreach (var title in taskTitles)
        {
            project.AddTask(title, Priority.Medium, null, TestClock.Now);
        }

        await using var context = _database.CreateContext();
        context.Projects.Add(project);
        await context.SaveChangesAsync(CancellationToken.None);

        return project;
    }

    private async Task<Project?> FindAsync(ProjectId id)
    {
        await using var context = _database.CreateContext();

        return await new ProjectRepository(context).FindAsync(id, CancellationToken.None);
    }
}
