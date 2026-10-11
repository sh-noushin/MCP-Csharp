using MediatR;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TaskTracker.Application.Projects.Commands;
using TaskTracker.Application.Projects.Queries;
using TaskTracker.Infrastructure.Persistence;
using TaskTracker.McpServer.Hosting;

namespace TaskTracker.McpServer.Tests.Hosting;

public sealed class TaskTrackerHostTests : IDisposable
{
    // A named in-memory database is shared by every connection until the last one closes.
    private readonly string _connectionString = $"Data Source=file:{Guid.NewGuid():N}?mode=memory&cache=shared";
    private readonly SqliteConnection _keepAlive;
    private readonly IHost _host;

    public TaskTrackerHostTests()
    {
        _keepAlive = new SqliteConnection(_connectionString);
        _keepAlive.Open();
        _host = TaskTrackerHost.Build([$"--ConnectionStrings:{DatabaseLocation.ConnectionStringName}={_connectionString}"]);
    }

    public void Dispose()
    {
        _host.Dispose();
        _keepAlive.Dispose();
    }

    [Fact]
    public async Task Build_WiresApplicationAndInfrastructureTogether()
    {
        await _host.Services.MigrateDatabaseAsync();

        Guid createdId;
        await using (var scope = _host.Services.CreateAsyncScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            createdId = (await sender.Send(new CreateProjectCommand("Website"))).Id;
        }

        await using var readScope = _host.Services.CreateAsyncScope();
        var listed = await readScope.ServiceProvider.GetRequiredService<ISender>().Send(new ListProjectsQuery());
        Assert.Equal(createdId, Assert.Single(listed).Id);
    }

    [Fact]
    public void Build_UsesTheConnectionStringFromTheCommandLine()
    {
        using var scope = _host.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TaskTrackerDbContext>();

        Assert.Equal(_connectionString, context.Database.GetConnectionString());
    }
}
