using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskTracker.Infrastructure.Persistence;

namespace TaskTracker.Infrastructure.Tests.Persistence;

public sealed class DatabaseMigratorTests : IDisposable
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");
    private readonly ServiceProvider _services;

    public DatabaseMigratorTests()
    {
        _connection.Open();
        _services = new ServiceCollection()
            .AddDbContext<TaskTrackerDbContext>(options => options.UseSqlite(_connection))
            .BuildServiceProvider();
    }

    public void Dispose()
    {
        _services.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public async Task MigrateDatabaseAsync_CreatesTheTables()
    {
        await _services.MigrateDatabaseAsync();

        Assert.Equal(["Projects", "Tasks"], await ReadTableNamesAsync());
    }

    [Fact]
    public async Task MigrateDatabaseAsync_CanRunAgainOnAnUpToDateDatabase()
    {
        await _services.MigrateDatabaseAsync();

        await _services.MigrateDatabaseAsync();

        Assert.Equal(["Projects", "Tasks"], await ReadTableNamesAsync());
    }

    private async Task<List<string>> ReadTableNamesAsync()
    {
        await using var scope = _services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<TaskTrackerDbContext>();

        return await context.Database
            .SqlQuery<string>($"SELECT name AS Value FROM sqlite_master WHERE name IN ('Projects', 'Tasks')")
            .OrderBy(name => name)
            .ToListAsync();
    }
}
