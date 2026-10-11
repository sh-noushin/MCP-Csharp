using TaskTracker.Application.Common;
using TaskTracker.Application.Projects.Commands;
using TaskTracker.Application.Projects.Services;
using TaskTracker.Domain.Projects;
using TaskTracker.TestSupport;

namespace TaskTracker.Application.Tests.Projects.Services;

public sealed class ArchiveProjectHandlerTests
{
    private readonly InMemoryProjectRepository _projects = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly ArchiveProjectHandler _handler;

    public ArchiveProjectHandlerTests() => _handler = new ArchiveProjectHandler(_projects, _unitOfWork);

    [Fact]
    public async Task HandleAsync_ArchivesProject()
    {
        var project = Project.Create("Website", TestClock.Now);
        _projects.Add(project);

        var result = await _handler.Handle(new ArchiveProjectCommand(project.Id.Value), CancellationToken.None);

        Assert.Equal("Archived", result.Status);
        Assert.Equal(ProjectStatus.Archived, project.Status);
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task HandleAsync_ReportsUnknownProject()
    {
        var unknownId = Guid.NewGuid();

        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(new ArchiveProjectCommand(unknownId), CancellationToken.None));

        Assert.Contains(unknownId.ToString(), exception.Message);
        Assert.Equal(0, _unitOfWork.SaveCount);
    }
}
