using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TaskTracker.Infrastructure.Persistence;

namespace TaskTracker.Infrastructure.Tests.Persistence;

/// <summary>An in-memory SQLite database that lives as long as its connection stays open.</summary>
internal sealed class SqliteTestDatabase : IDisposable
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");

    public SqliteTestDatabase()
    {
        _connection.Open();

        using var context = CreateContext();
        context.Database.Migrate();
    }

    public TaskTrackerDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<TaskTrackerDbContext>().UseSqlite(_connection).Options);

    public void Dispose() => _connection.Dispose();
}
