using System.ComponentModel;
using MediatR;
using ModelContextProtocol.Server;
using TaskTracker.Application.Tasks.Commands;
using TaskTracker.Application.Tasks.Dtos;
using TaskTracker.Application.Tasks.Queries;
using TaskTracker.Domain.Tasks;

namespace TaskTracker.McpServer.Tools;

[McpServerToolType]
public sealed class TaskTools(ISender sender)
{
    [McpServerTool(Name = "add_task")]
    [Description("Adds a task to an active project.")]
    public Task<TaskDto> AddTaskAsync(
        [Description("Id of the project the task belongs to.")] Guid projectId,
        [Description("Short title of the task.")] string title,
        [Description("Low, Medium or High.")] string priority = nameof(Priority.Medium),
        [Description("Optional due date as yyyy-MM-dd. Cannot be in the past.")] DateOnly? dueDate = null,
        CancellationToken cancellationToken = default) =>
        ToolExecution.RunAsync(() => sender.Send(
            new AddTaskCommand(projectId, title, EnumArgument.Parse<Priority>(priority, "priority"), dueDate),
            cancellationToken));

    [McpServerTool(Name = "start_task")]
    [Description("Moves a task from Todo to InProgress.")]
    public Task<TaskDto> StartTaskAsync(
        [Description("Id of the task to start.")] Guid taskId,
        CancellationToken cancellationToken) =>
        ToolExecution.RunAsync(() => sender.Send(new StartTaskCommand(taskId), cancellationToken));

    [McpServerTool(Name = "complete_task")]
    [Description("Marks a task as Done.")]
    public Task<TaskDto> CompleteTaskAsync(
        [Description("Id of the task to complete.")] Guid taskId,
        CancellationToken cancellationToken) =>
        ToolExecution.RunAsync(() => sender.Send(new CompleteTaskCommand(taskId), cancellationToken));

    [McpServerTool(Name = "list_tasks", ReadOnly = true)]
    [Description("Lists tasks ordered by due date, optionally filtered by project, status or overdue.")]
    public Task<IReadOnlyList<TaskDto>> ListTasksAsync(
        [Description("Only return tasks of this project.")] Guid? projectId = null,
        [Description("Only return tasks with this status: Todo, InProgress or Done.")] string? status = null,
        [Description("Only return unfinished tasks whose due date has passed.")] bool overdueOnly = false,
        CancellationToken cancellationToken = default) =>
        ToolExecution.RunAsync(() => sender.Send(
            new ListTasksQuery(projectId, ParseStatus(status), overdueOnly),
            cancellationToken));

    private static TaskItemStatus? ParseStatus(string? status) =>
        string.IsNullOrWhiteSpace(status) ? null : EnumArgument.Parse<TaskItemStatus>(status, "status");
}
