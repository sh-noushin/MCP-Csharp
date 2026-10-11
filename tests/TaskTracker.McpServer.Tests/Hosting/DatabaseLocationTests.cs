using Microsoft.Extensions.Configuration;
using TaskTracker.McpServer.Hosting;

namespace TaskTracker.McpServer.Tests.Hosting;

public sealed class DatabaseLocationTests : IDisposable
{
    private readonly string _baseDirectory = Path.Combine(Path.GetTempPath(), $"tasktracker-tests-{Guid.NewGuid():N}");

    public void Dispose()
    {
        if (Directory.Exists(_baseDirectory))
        {
            Directory.Delete(_baseDirectory, recursive: true);
        }
    }

    [Fact]
    public void ResolveConnectionString_PrefersTheConfiguredConnectionString()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"ConnectionStrings:{DatabaseLocation.ConnectionStringName}"] = "Data Source=custom.db",
            })
            .Build();

        Assert.Equal("Data Source=custom.db", DatabaseLocation.ResolveConnectionString(configuration));
    }

    [Fact]
    public void BuildConnectionString_PointsAtADatabaseFileInTheApplicationFolder()
    {
        var connectionString = DatabaseLocation.BuildConnectionString(_baseDirectory);

        var expectedFile = Path.Combine(_baseDirectory, "TaskTracker", "tasktracker.db");
        Assert.Equal($"Data Source={expectedFile}", connectionString);
    }

    [Fact]
    public void BuildConnectionString_CreatesTheApplicationFolder()
    {
        DatabaseLocation.BuildConnectionString(_baseDirectory);

        Assert.True(Directory.Exists(Path.Combine(_baseDirectory, "TaskTracker")));
    }
}
