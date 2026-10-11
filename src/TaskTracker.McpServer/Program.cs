using Microsoft.Extensions.Hosting;
using TaskTracker.Infrastructure.Persistence;
using TaskTracker.McpServer.Hosting;

using var host = TaskTrackerHost.Build(args);

await host.Services.MigrateDatabaseAsync();
await host.RunAsync();
