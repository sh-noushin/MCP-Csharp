using System.ComponentModel;
using MediatR;
using ModelContextProtocol.Server;
using TaskTracker.Application.Projects.Commands;
using TaskTracker.Application.Projects.Dtos;
using TaskTracker.Application.Projects.Queries;

namespace TaskTracker.McpServer.Tools;

[McpServerToolType]
public sealed class ProjectTools(ISender sender)
{
    [McpServerTool(Name = "create_project")]
    [Description("Creates a project that tasks can be added to.")]
    public Task<ProjectDto> CreateProjectAsync(
        [Description("Name of the project.")] string name,
        CancellationToken cancellationToken) =>
        ToolExecution.RunAsync(() => sender.Send(new CreateProjectCommand(name), cancellationToken));

    [McpServerTool(Name = "list_projects", ReadOnly = true)]
    [Description("Lists projects with their task counts. Archived projects are hidden unless requested.")]
    public Task<IReadOnlyList<ProjectDto>> ListProjectsAsync(
        [Description("Also return archived projects.")] bool includeArchived = false,
        CancellationToken cancellationToken = default) =>
        ToolExecution.RunAsync(() => sender.Send(new ListProjectsQuery(includeArchived), cancellationToken));

    [McpServerTool(Name = "archive_project")]
    [Description("Archives a project. An archived project and its tasks can no longer be changed.")]
    public Task<ProjectDto> ArchiveProjectAsync(
        [Description("Id of the project to archive.")] Guid projectId,
        CancellationToken cancellationToken) =>
        ToolExecution.RunAsync(() => sender.Send(new ArchiveProjectCommand(projectId), cancellationToken));
}
