using MediatR;
using TaskTracker.Application.Projects.Dtos;
using TaskTracker.Application.Projects.Queries;
using TaskTracker.Domain.Projects;

namespace TaskTracker.Application.Projects.Services;

public sealed class ListProjectsHandler(IProjectRepository projects)
    : IRequestHandler<ListProjectsQuery, IReadOnlyList<ProjectDto>>
{
    public async Task<IReadOnlyList<ProjectDto>> Handle(
        ListProjectsQuery query,
        CancellationToken cancellationToken)
    {
        var all = await projects.ListAsync(cancellationToken);

        return
        [
            .. all
                .Where(project => query.IncludeArchived || project.Status == ProjectStatus.Active)
                .OrderBy(project => project.Name.Value, StringComparer.OrdinalIgnoreCase)
                .Select(project => project.ToDto()),
        ];
    }
}
