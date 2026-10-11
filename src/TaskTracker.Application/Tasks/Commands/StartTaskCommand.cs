using MediatR;
using TaskTracker.Application.Tasks.Dtos;

namespace TaskTracker.Application.Tasks.Commands;

public sealed record StartTaskCommand(Guid TaskId) : IRequest<TaskDto>;
