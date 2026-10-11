using ModelContextProtocol;
using TaskTracker.McpServer.Tools;

namespace TaskTracker.McpServer.Tests.Tools;

public sealed class ProjectToolsTests : IDisposable
{
    private readonly ToolTestContext _context = new();
    private readonly ProjectTools _tools;

    public ProjectToolsTests() => _tools = new ProjectTools(_context.Sender);

    public void Dispose() => _context.Dispose();

    [Fact]
    public async Task CreateProject_StoresAndReturnsTheProject()
    {
        var created = await _tools.CreateProjectAsync("Website", CancellationToken.None);

        Assert.Equal("Website", created.Name);
        Assert.Equal("Active", created.Status);
        Assert.Equal(created.Id, Assert.Single(_context.Projects.Projects).Id.Value);
    }

    [Fact]
    public async Task CreateProject_ReportsABusinessRuleFailureToTheClient()
    {
        var exception = await Assert.ThrowsAsync<McpException>(
            () => _tools.CreateProjectAsync("  ", CancellationToken.None));

        Assert.Equal("Project name is required.", exception.Message);
    }

    [Fact]
    public async Task ListProjects_HidesArchivedProjectsByDefault()
    {
        await _tools.CreateProjectAsync("Website", CancellationToken.None);
        var old = await _tools.CreateProjectAsync("Old", CancellationToken.None);
        await _tools.ArchiveProjectAsync(old.Id, CancellationToken.None);

        var listed = await _tools.ListProjectsAsync();

        Assert.Equal("Website", Assert.Single(listed).Name);
    }

    [Fact]
    public async Task ListProjects_IncludesArchivedProjectsWhenAsked()
    {
        await _tools.CreateProjectAsync("Website", CancellationToken.None);
        var old = await _tools.CreateProjectAsync("Old", CancellationToken.None);
        await _tools.ArchiveProjectAsync(old.Id, CancellationToken.None);

        var listed = await _tools.ListProjectsAsync(includeArchived: true);

        Assert.Equal(["Old", "Website"], listed.Select(project => project.Name));
    }

    [Fact]
    public async Task ArchiveProject_ReturnsTheArchivedProject()
    {
        var created = await _tools.CreateProjectAsync("Website", CancellationToken.None);

        var archived = await _tools.ArchiveProjectAsync(created.Id, CancellationToken.None);

        Assert.Equal("Archived", archived.Status);
    }

    [Fact]
    public async Task ArchiveProject_ReportsAnUnknownProjectToTheClient()
    {
        var unknownId = Guid.NewGuid();

        var exception = await Assert.ThrowsAsync<McpException>(
            () => _tools.ArchiveProjectAsync(unknownId, CancellationToken.None));

        Assert.Equal($"Project '{unknownId}' was not found.", exception.Message);
    }
}
