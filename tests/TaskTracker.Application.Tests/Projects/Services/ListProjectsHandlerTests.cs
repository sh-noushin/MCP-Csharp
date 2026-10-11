using TaskTracker.Application.Projects.Queries;
using TaskTracker.Application.Projects.Services;
using TaskTracker.Domain.Projects;
using TaskTracker.Domain.Tasks;
using TaskTracker.TestSupport;

namespace TaskTracker.Application.Tests.Projects.Services;

public sealed class ListProjectsHandlerTests
{
    private readonly InMemoryProjectRepository _projects = new();
    private readonly ListProjectsHandler _handler;

    public ListProjectsHandlerTests() => _handler = new ListProjectsHandler(_projects);

    [Fact]
    public async Task HandleAsync_ReturnsActiveProjectsOrderedByName()
    {
        AddProject("zebra");
        AddProject("Alpha");
        AddProject("Old").Archive();

        var result = await _handler.Handle(new ListProjectsQuery(), CancellationToken.None);

        Assert.Equal(["Alpha", "zebra"], result.Select(project => project.Name));
    }

    [Fact]
    public async Task HandleAsync_IncludesArchivedProjectsWhenAsked()
    {
        AddProject("Alpha");
        AddProject("Old").Archive();

        var result = await _handler.Handle(new ListProjectsQuery(IncludeArchived: true), CancellationToken.None);

        Assert.Equal(["Alpha", "Old"], result.Select(project => project.Name));
    }

    [Fact]
    public async Task HandleAsync_CountsAllAndOpenTasks()
    {
        var project = AddProject("Alpha");
        var done = project.AddTask("Done one", Priority.Low, null, TestClock.Now);
        project.AddTask("Open one", Priority.Low, null, TestClock.Now);
        project.CompleteTask(done.Id, TestClock.Now);

        var result = await _handler.Handle(new ListProjectsQuery(), CancellationToken.None);

        var dto = Assert.Single(result);
        Assert.Equal(2, dto.TaskCount);
        Assert.Equal(1, dto.OpenTaskCount);
    }

    private Project AddProject(string name)
    {
        var project = Project.Create(name, TestClock.Now);
        _projects.Add(project);

        return project;
    }
}
