using Microsoft.EntityFrameworkCore;
using TaskTracker.Domain.Projects;
using TaskTracker.Domain.Tasks;

namespace TaskTracker.Infrastructure.Persistence.Repositories;

internal sealed class ProjectRepository(TaskTrackerDbContext context) : IProjectRepository
{
    public void Add(Project project) => context.Projects.Add(project);

    public Task<Project?> FindAsync(ProjectId id, CancellationToken cancellationToken) =>
        WithTasks().FirstOrDefaultAsync(project => project.Id == id, cancellationToken);

    public Task<Project?> FindByTaskAsync(TaskItemId taskId, CancellationToken cancellationToken) =>
        WithTasks().FirstOrDefaultAsync(
            project => project.Tasks.Any(task => task.Id == taskId),
            cancellationToken);

    public async Task<IReadOnlyList<Project>> ListAsync(CancellationToken cancellationToken) =>
        await WithTasks().ToListAsync(cancellationToken);

    private IQueryable<Project> WithTasks() => context.Projects.Include(project => project.Tasks);
}
