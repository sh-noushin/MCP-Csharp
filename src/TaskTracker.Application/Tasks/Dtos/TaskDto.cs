namespace TaskTracker.Application.Tasks.Dtos;

public sealed record TaskDto(
    Guid Id,
    Guid ProjectId,
    string Title,
    string Priority,
    string Status,
    DateOnly? DueDate,
    bool IsOverdue,
    DateTimeOffset CreatedAt,
    DateTimeOffset? CompletedAt);
