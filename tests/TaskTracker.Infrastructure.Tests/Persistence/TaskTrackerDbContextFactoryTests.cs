using Microsoft.EntityFrameworkCore;
using TaskTracker.Infrastructure.Persistence;

namespace TaskTracker.Infrastructure.Tests.Persistence;

public sealed class TaskTrackerDbContextFactoryTests
{
    [Fact]
    public void CreateDbContext_ReturnsAContextConfiguredForSqlite()
    {
        using var context = new TaskTrackerDbContextFactory().CreateDbContext([]);

        Assert.True(context.Database.IsSqlite());
    }
}
