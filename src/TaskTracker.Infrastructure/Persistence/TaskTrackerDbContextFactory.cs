using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TaskTracker.Infrastructure.Persistence;

/// <summary>Lets the EF Core tools create the context when adding migrations.</summary>
public sealed class TaskTrackerDbContextFactory : IDesignTimeDbContextFactory<TaskTrackerDbContext>
{
    private const string DesignTimeConnectionString = "Data Source=design-time.db";

    public TaskTrackerDbContext CreateDbContext(string[] args) =>
        new(new DbContextOptionsBuilder<TaskTrackerDbContext>().UseSqlite(DesignTimeConnectionString).Options);
}
