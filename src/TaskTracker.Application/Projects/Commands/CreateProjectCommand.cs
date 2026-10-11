using MediatR;
using TaskTracker.Application.Projects.Dtos;

namespace TaskTracker.Application.Projects.Commands;

public sealed record CreateProjectCommand(string Name) : IRequest<ProjectDto>;
