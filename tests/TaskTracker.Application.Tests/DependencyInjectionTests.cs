using MediatR;
using Microsoft.Extensions.DependencyInjection;
using TaskTracker.Application.Abstractions;
using TaskTracker.Application.Projects.Commands;
using TaskTracker.Application.Projects.Dtos;
using TaskTracker.Application.Projects.Queries;
using TaskTracker.Application.Tasks.Commands;
using TaskTracker.Application.Tasks.Dtos;
using TaskTracker.Application.Tasks.Queries;
using TaskTracker.Domain.Projects;
using TaskTracker.TestSupport;

namespace TaskTracker.Application.Tests;

public sealed class DependencyInjectionTests
{
    [Theory]
    [InlineData(typeof(IRequestHandler<CreateProjectCommand, ProjectDto>))]
    [InlineData(typeof(IRequestHandler<ArchiveProjectCommand, ProjectDto>))]
    [InlineData(typeof(IRequestHandler<ListProjectsQuery, IReadOnlyList<ProjectDto>>))]
    [InlineData(typeof(IRequestHandler<AddTaskCommand, TaskDto>))]
    [InlineData(typeof(IRequestHandler<StartTaskCommand, TaskDto>))]
    [InlineData(typeof(IRequestHandler<CompleteTaskCommand, TaskDto>))]
    [InlineData(typeof(IRequestHandler<ListTasksQuery, IReadOnlyList<TaskDto>>))]
    public void AddApplication_RegistersAHandlerForEveryRequest(Type handlerType)
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        Assert.NotNull(scope.ServiceProvider.GetService(handlerType));
    }

    [Fact]
    public async Task AddApplication_LetsTheMediatorRouteARequestToItsHandler()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var created = await sender.Send(new CreateProjectCommand("Website"));
        var listed = await sender.Send(new ListProjectsQuery());

        Assert.Equal(created, Assert.Single(listed));
    }

    [Fact]
    public void AddApplication_KeepsAnAlreadyRegisteredTimeProvider()
    {
        var timeProvider = TestClock.CreateProvider();

        using var provider = new ServiceCollection()
            .AddSingleton<TimeProvider>(timeProvider)
            .AddApplication()
            .BuildServiceProvider();

        Assert.Same(timeProvider, provider.GetRequiredService<TimeProvider>());
    }

    private static ServiceProvider BuildProvider() =>
        new ServiceCollection()
            .AddLogging()
            .AddApplication()
            .AddSingleton<IProjectRepository, InMemoryProjectRepository>()
            .AddSingleton<IUnitOfWork, FakeUnitOfWork>()
            .BuildServiceProvider(validateScopes: true);
}
