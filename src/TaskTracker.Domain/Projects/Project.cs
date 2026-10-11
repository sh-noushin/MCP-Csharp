using TaskTracker.Domain.Common;
using TaskTracker.Domain.Tasks;

namespace TaskTracker.Domain.Projects;

public sealed class Project
{
    private readonly List<TaskItem> _tasks = [];

    private Project(ProjectId id, ProjectName name, DateTimeOffset createdAt)
    {
        Id = id;
        Name = name;
        CreatedAt = createdAt;
        Status = ProjectStatus.Active;
    }

    public ProjectId Id { get; }

    public ProjectName Name { get; }

    public ProjectStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; }

    public IReadOnlyCollection<TaskItem> Tasks => _tasks.AsReadOnly();

    public static Project Create(string? name, DateTimeOffset now) =>
        new(ProjectId.New(), ProjectName.Create(name), now);

    public TaskItem AddTask(string? title, Priority priority, DateOnly? dueDate, DateTimeOffset now)
    {
        EnsureActive();

        var task = TaskItem.Create(Id, title, priority, dueDate, now);
        _tasks.Add(task);

        return task;
    }

    public TaskItem StartTask(TaskItemId taskId)
    {
        EnsureActive();

        var task = GetTask(taskId);
        task.Start();

        return task;
    }

    public TaskItem CompleteTask(TaskItemId taskId, DateTimeOffset now)
    {
        EnsureActive();

        var task = GetTask(taskId);
        task.Complete(now);

        return task;
    }

    public void Archive()
    {
        EnsureActive();
        Status = ProjectStatus.Archived;
    }

    private void EnsureActive()
    {
        if (Status == ProjectStatus.Archived)
        {
            throw new DomainException($"Project '{Name}' is archived and cannot be changed.");
        }
    }

    private TaskItem GetTask(TaskItemId taskId) =>
        _tasks.Find(task => task.Id == taskId)
        ?? throw new DomainException($"Task '{taskId}' does not belong to project '{Name}'.");
}
