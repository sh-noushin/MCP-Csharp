using MediatR;
using TaskTracker.Application.Tasks.Dtos;
using TaskTracker.Domain.Tasks;

namespace TaskTracker.Application.Tasks.Commands;

public sealed record AddTaskCommand(Guid ProjectId, string Title, Priority Priority, DateOnly? DueDate)
    : IRequest<TaskDto>;
