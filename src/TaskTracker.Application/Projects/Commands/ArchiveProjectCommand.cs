using MediatR;
using TaskTracker.Application.Projects.Dtos;

namespace TaskTracker.Application.Projects.Commands;

public sealed record ArchiveProjectCommand(Guid ProjectId) : IRequest<ProjectDto>;
