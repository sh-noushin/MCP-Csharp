using TaskTracker.Application.Projects.Commands;
using TaskTracker.Application.Projects.Services;
using TaskTracker.Domain.Common;
using TaskTracker.TestSupport;

namespace TaskTracker.Application.Tests.Projects.Services;

public sealed class CreateProjectHandlerTests
{
    private readonly InMemoryProjectRepository _projects = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly CreateProjectHandler _handler;

    public CreateProjectHandlerTests() =>
        _handler = new CreateProjectHandler(_projects, _unitOfWork, TestClock.CreateProvider());

    [Fact]
    public async Task HandleAsync_StoresProjectAndReturnsIt()
    {
        var result = await _handler.Handle(new CreateProjectCommand("Website"), CancellationToken.None);

        var stored = Assert.Single(_projects.Projects);
        Assert.Equal(stored.Id.Value, result.Id);
        Assert.Equal("Website", result.Name);
        Assert.Equal("Active", result.Status);
        Assert.Equal(TestClock.Now, result.CreatedAt);
        Assert.Equal(0, result.TaskCount);
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task HandleAsync_DoesNotSaveInvalidProject()
    {
        await Assert.ThrowsAsync<DomainException>(
            () => _handler.Handle(new CreateProjectCommand(" "), CancellationToken.None));

        Assert.Empty(_projects.Projects);
        Assert.Equal(0, _unitOfWork.SaveCount);
    }
}
