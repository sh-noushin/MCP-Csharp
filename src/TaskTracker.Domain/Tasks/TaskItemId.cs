namespace TaskTracker.Domain.Tasks;

public readonly record struct TaskItemId(Guid Value)
{
    public static TaskItemId New() => new(Guid.CreateVersion7());

    public override string ToString() => Value.ToString();
}
