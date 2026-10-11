using MediatR;
using TaskTracker.Application.Common;
using TaskTracker.Application.Tasks.Dtos;
using TaskTracker.Application.Tasks.Queries;
using TaskTracker.Domain.Projects;

namespace TaskTracker.Application.Tasks.Services;

public sealed class ListTasksHandler(IProjectRepository projects, TimeProvider timeProvider)
    : IRequestHandler<ListTasksQuery, IReadOnlyList<TaskDto>>
{
    public async Task<IReadOnlyList<TaskDto>> Handle(ListTasksQuery query, CancellationToken cancellationToken)
    {
        var today = timeProvider.Today;
        var all = await projects.ListAsync(cancellationToken);

        return
        [
            .. all
                .Where(project => query.ProjectId is null || project.Id.Value == query.ProjectId)
                .SelectMany(project => project.Tasks)
                .Where(task => query.Status is null || task.Status == query.Status)
                .Where(task => !query.OverdueOnly || task.IsOverdue(today))
                .OrderBy(task => task.DueDate ?? DateOnly.MaxValue)
                .ThenByDescending(task => task.Priority)
                .Select(task => task.ToDto(today)),
        ];
    }
}
