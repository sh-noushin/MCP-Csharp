using MediatR;
using TaskTracker.Application.Abstractions;
using TaskTracker.Application.Projects.Commands;
using TaskTracker.Application.Projects.Dtos;
using TaskTracker.Domain.Projects;

namespace TaskTracker.Application.Projects.Services;

public sealed class CreateProjectHandler(
    IProjectRepository projects,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : IRequestHandler<CreateProjectCommand, ProjectDto>
{
    public async Task<ProjectDto> Handle(CreateProjectCommand command, CancellationToken cancellationToken)
    {
        var project = Project.Create(command.Name, timeProvider.GetLocalNow());

        projects.Add(project);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return project.ToDto();
    }
}
