using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using TaskTracker.Application;
using TaskTracker.Application.Abstractions;
using TaskTracker.Domain.Projects;
using TaskTracker.TestSupport;

namespace TaskTracker.McpServer.Tests.Tools;

/// <summary>The real application layer on top of in-memory fakes, so tools are tested without a database.</summary>
internal sealed class ToolTestContext : IDisposable
{
    private readonly ServiceProvider _services;

    public ToolTestContext() =>
        _services = new ServiceCollection()
            .AddLogging()
            .AddSingleton<TimeProvider>(TimeProvider)
            .AddSingleton<IProjectRepository>(Projects)
            .AddSingleton<IUnitOfWork, FakeUnitOfWork>()
            .AddApplication()
            .BuildServiceProvider();

    public FakeTimeProvider TimeProvider { get; } = TestClock.CreateProvider();

    public InMemoryProjectRepository Projects { get; } = new();

    public ISender Sender => _services.GetRequiredService<ISender>();

    public void Dispose() => _services.Dispose();
}
