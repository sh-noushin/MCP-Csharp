namespace TaskTracker.Application.Projects.Dtos;

public sealed record ProjectDto(
    Guid Id,
    string Name,
    string Status,
    DateTimeOffset CreatedAt,
    int TaskCount,
    int OpenTaskCount);
