using MediatR;
using TaskTracker.Application.Abstractions;
using TaskTracker.Application.Common;
using TaskTracker.Application.Projects.Commands;
using TaskTracker.Application.Projects.Dtos;
using TaskTracker.Domain.Projects;

namespace TaskTracker.Application.Projects.Services;

public sealed class ArchiveProjectHandler(IProjectRepository projects, IUnitOfWork unitOfWork)
    : IRequestHandler<ArchiveProjectCommand, ProjectDto>
{
    public async Task<ProjectDto> Handle(ArchiveProjectCommand command, CancellationToken cancellationToken)
    {
        var project = await projects.GetAsync(command.ProjectId, cancellationToken);

        project.Archive();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return project.ToDto();
    }
}
