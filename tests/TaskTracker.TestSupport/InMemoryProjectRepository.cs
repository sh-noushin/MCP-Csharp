using TaskTracker.Domain.Projects;
using TaskTracker.Domain.Tasks;

namespace TaskTracker.TestSupport;

public sealed class InMemoryProjectRepository : IProjectRepository
{
    private readonly List<Project> _projects = [];

    public IReadOnlyList<Project> Projects => _projects;

    public void Add(Project project) => _projects.Add(project);

    public Task<Project?> FindAsync(ProjectId id, CancellationToken cancellationToken) =>
        Task.FromResult(_projects.Find(project => project.Id == id));

    public Task<Project?> FindByTaskAsync(TaskItemId taskId, CancellationToken cancellationToken) =>
        Task.FromResult(_projects.Find(project => project.Tasks.Any(task => task.Id == taskId)));

    public Task<IReadOnlyList<Project>> ListAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Project>>([.. _projects]);
}
