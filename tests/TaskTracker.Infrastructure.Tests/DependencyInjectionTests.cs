using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using TaskTracker.Application.Abstractions;
using TaskTracker.Domain.Projects;
using TaskTracker.Infrastructure.Persistence;
using TaskTracker.Infrastructure.Persistence.Repositories;
using TaskTracker.TestSupport;

namespace TaskTracker.Infrastructure.Tests;

public sealed class DependencyInjectionTests : IDisposable
{
    // A named in-memory database is shared by every connection until the last one closes.
    private readonly string _connectionString = $"Data Source=file:{Guid.NewGuid():N}?mode=memory&cache=shared";
    private readonly SqliteConnection _keepAlive;
    private readonly ServiceProvider _services;

    public DependencyInjectionTests()
    {
        _keepAlive = new SqliteConnection(_connectionString);
        _keepAlive.Open();
        _services = new ServiceCollection()
            .AddInfrastructure(_connectionString)
            .BuildServiceProvider(validateScopes: true);
    }

    public void Dispose()
    {
        _services.Dispose();
        _keepAlive.Dispose();
    }

    [Fact]
    public void AddInfrastructure_RegistersTheEfCoreRepository()
    {
        using var scope = _services.CreateScope();

        Assert.IsType<ProjectRepository>(scope.ServiceProvider.GetRequiredService<IProjectRepository>());
    }

    [Fact]
    public void AddInfrastructure_UsesTheDbContextAsTheUnitOfWork()
    {
        using var scope = _services.CreateScope();

        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        Assert.Same(scope.ServiceProvider.GetRequiredService<TaskTrackerDbContext>(), unitOfWork);
    }

    [Fact]
    public void AddInfrastructure_GivesEachScopeItsOwnDbContext()
    {
        using var first = _services.CreateScope();
        using var second = _services.CreateScope();

        Assert.NotSame(
            first.ServiceProvider.GetRequiredService<TaskTrackerDbContext>(),
            second.ServiceProvider.GetRequiredService<TaskTrackerDbContext>());
    }

    [Fact]
    public async Task AddInfrastructure_SavesWhatTheRepositoryAddedThroughTheUnitOfWork()
    {
        await _services.MigrateDatabaseAsync();
        var project = Project.Create("Website", TestClock.Now);

        await using (var scope = _services.CreateAsyncScope())
        {
            scope.ServiceProvider.GetRequiredService<IProjectRepository>().Add(project);
            await scope.ServiceProvider.GetRequiredService<IUnitOfWork>().SaveChangesAsync(CancellationToken.None);
        }

        await using var readScope = _services.CreateAsyncScope();
        var repository = readScope.ServiceProvider.GetRequiredService<IProjectRepository>();
        Assert.NotNull(await repository.FindAsync(project.Id, CancellationToken.None));
    }
}
