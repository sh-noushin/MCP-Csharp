using MediatR;
using TaskTracker.Application.Abstractions;
using TaskTracker.Application.Common;
using TaskTracker.Application.Tasks.Commands;
using TaskTracker.Application.Tasks.Dtos;
using TaskTracker.Domain.Projects;

namespace TaskTracker.Application.Tasks.Services;

public sealed class AddTaskHandler(
    IProjectRepository projects,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : IRequestHandler<AddTaskCommand, TaskDto>
{
    public async Task<TaskDto> Handle(AddTaskCommand command, CancellationToken cancellationToken)
    {
        var project = await projects.GetAsync(command.ProjectId, cancellationToken);

        var task = project.AddTask(command.Title, command.Priority, command.DueDate, timeProvider.GetLocalNow());
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return task.ToDto(timeProvider.Today);
    }
}
