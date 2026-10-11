using TaskTracker.Domain.Tasks;

namespace TaskTracker.Application.Tasks.Dtos;

internal static class TaskMappings
{
    public static TaskDto ToDto(this TaskItem task, DateOnly today) =>
        new(
            task.Id.Value,
            task.ProjectId.Value,
            task.Title.Value,
            task.Priority.ToString(),
            task.Status.ToString(),
            task.DueDate,
            task.IsOverdue(today),
            task.CreatedAt,
            task.CompletedAt);
}
