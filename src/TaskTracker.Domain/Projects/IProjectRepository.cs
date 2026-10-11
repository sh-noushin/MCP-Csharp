using TaskTracker.Domain.Tasks;

namespace TaskTracker.Domain.Projects;

public interface IProjectRepository
{
    void Add(Project project);

    Task<Project?> FindAsync(ProjectId id, CancellationToken cancellationToken);

    Task<Project?> FindByTaskAsync(TaskItemId taskId, CancellationToken cancellationToken);

    Task<IReadOnlyList<Project>> ListAsync(CancellationToken cancellationToken);
}
