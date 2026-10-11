using TaskTracker.Domain.Common;
using TaskTracker.Domain.Projects;

namespace TaskTracker.Domain.Tasks;

public sealed class TaskItem
{
    private TaskItem(
        TaskItemId id,
        ProjectId projectId,
        TaskTitle title,
        Priority priority,
        DateOnly? dueDate,
        DateTimeOffset createdAt)
    {
        Id = id;
        ProjectId = projectId;
        Title = title;
        Priority = priority;
        DueDate = dueDate;
        CreatedAt = createdAt;
        Status = TaskItemStatus.Todo;
    }

    public TaskItemId Id { get; }

    public ProjectId ProjectId { get; }

    public TaskTitle Title { get; }

    public Priority Priority { get; }

    public DateOnly? DueDate { get; }

    public TaskItemStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; }

    public DateTimeOffset? CompletedAt { get; private set; }

    public bool IsOverdue(DateOnly today) => Status != TaskItemStatus.Done && DueDate < today;

    internal static TaskItem Create(
        ProjectId projectId,
        string? title,
        Priority priority,
        DateOnly? dueDate,
        DateTimeOffset now)
    {
        if (!Enum.IsDefined(priority))
        {
            throw new DomainException($"'{priority}' is not a valid priority.");
        }

        if (dueDate < DateOnly.FromDateTime(now.DateTime))
        {
            throw new DomainException("Due date cannot be in the past.");
        }

        return new TaskItem(TaskItemId.New(), projectId, TaskTitle.Create(title), priority, dueDate, now);
    }

    internal void Start()
    {
        if (Status != TaskItemStatus.Todo)
        {
            throw new DomainException($"Task '{Title}' cannot be started because it is {Status}.");
        }

        Status = TaskItemStatus.InProgress;
    }

    internal void Complete(DateTimeOffset now)
    {
        if (Status == TaskItemStatus.Done)
        {
            throw new DomainException($"Task '{Title}' is already done.");
        }

        Status = TaskItemStatus.Done;
        CompletedAt = now;
    }
}
