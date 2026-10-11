using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TaskTracker.Application;
using TaskTracker.Infrastructure;

namespace TaskTracker.McpServer.Hosting;

public static class TaskTrackerHost
{
    public static IHost Build(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        // stdout carries the MCP protocol, so every log line has to go to stderr.
        builder.Logging.AddConsole(options => options.LogToStandardErrorThreshold = LogLevel.Trace);

        builder.Services
            .AddApplication()
            .AddInfrastructure(DatabaseLocation.ResolveConnectionString(builder.Configuration))
            .AddMcpServer()
            .WithStdioServerTransport();

        return builder.Build();
    }
}
