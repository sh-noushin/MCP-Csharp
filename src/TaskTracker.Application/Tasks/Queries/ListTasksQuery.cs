using MediatR;
using TaskTracker.Application.Tasks.Dtos;
using TaskTracker.Domain.Tasks;

namespace TaskTracker.Application.Tasks.Queries;

public sealed record ListTasksQuery(Guid? ProjectId = null, TaskItemStatus? Status = null, bool OverdueOnly = false)
    : IRequest<IReadOnlyList<TaskDto>>;
