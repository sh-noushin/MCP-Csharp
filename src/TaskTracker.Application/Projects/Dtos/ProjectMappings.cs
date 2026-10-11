using TaskTracker.Domain.Projects;
using TaskTracker.Domain.Tasks;

namespace TaskTracker.Application.Projects.Dtos;

internal static class ProjectMappings
{
    public static ProjectDto ToDto(this Project project) =>
        new(
            project.Id.Value,
            project.Name.Value,
            project.Status.ToString(),
            project.CreatedAt,
            project.Tasks.Count,
            project.Tasks.Count(task => task.Status != TaskItemStatus.Done));
}
