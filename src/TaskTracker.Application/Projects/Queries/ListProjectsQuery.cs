using MediatR;
using TaskTracker.Application.Projects.Dtos;

namespace TaskTracker.Application.Projects.Queries;

public sealed record ListProjectsQuery(bool IncludeArchived = false) : IRequest<IReadOnlyList<ProjectDto>>;
