using Microsoft.Extensions.Configuration;

namespace TaskTracker.McpServer.Hosting;

public static class DatabaseLocation
{
    public const string ConnectionStringName = "TaskTracker";

    private const string ApplicationFolder = "TaskTracker";
    private const string DatabaseFileName = "tasktracker.db";

    public static string ResolveConnectionString(IConfiguration configuration) =>
        configuration.GetConnectionString(ConnectionStringName)
        ?? BuildConnectionString(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));

    public static string BuildConnectionString(string baseDirectory)
    {
        var directory = Path.Combine(baseDirectory, ApplicationFolder);
        Directory.CreateDirectory(directory);

        return $"Data Source={Path.Combine(directory, DatabaseFileName)}";
    }
}
