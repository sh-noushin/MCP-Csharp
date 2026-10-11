using MediatR;
using TaskTracker.Application.Abstractions;
using TaskTracker.Application.Common;
using TaskTracker.Application.Tasks.Commands;
using TaskTracker.Application.Tasks.Dtos;
using TaskTracker.Domain.Projects;
using TaskTracker.Domain.Tasks;

namespace TaskTracker.Application.Tasks.Services;

public sealed class CompleteTaskHandler(
    IProjectRepository projects,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : IRequestHandler<CompleteTaskCommand, TaskDto>
{
    public async Task<TaskDto> Handle(CompleteTaskCommand command, CancellationToken cancellationToken)
    {
        var taskId = new TaskItemId(command.TaskId);
        var project = await projects.GetByTaskAsync(taskId, cancellationToken);

        var task = project.CompleteTask(taskId, timeProvider.GetLocalNow());
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return task.ToDto(timeProvider.Today);
    }
}
