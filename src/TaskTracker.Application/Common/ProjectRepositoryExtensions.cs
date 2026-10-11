using TaskTracker.Domain.Projects;
using TaskTracker.Domain.Tasks;

namespace TaskTracker.Application.Common;

internal static class ProjectRepositoryExtensions
{
    public static async Task<Project> GetAsync(
        this IProjectRepository projects,
        Guid projectId,
        CancellationToken cancellationToken) =>
        await projects.FindAsync(new ProjectId(projectId), cancellationToken)
        ?? throw new NotFoundException($"Project '{projectId}' was not found.");

    public static async Task<Project> GetByTaskAsync(
        this IProjectRepository projects,
        TaskItemId taskId,
        CancellationToken cancellationToken) =>
        await projects.FindByTaskAsync(taskId, cancellationToken)
        ?? throw new NotFoundException($"Task '{taskId}' was not found.");
}
