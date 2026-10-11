using TaskTracker.Domain.Common;

namespace TaskTracker.Domain.Tasks;

public sealed record TaskTitle
{
    public const int MaxLength = 200;

    private TaskTitle(string value) => Value = value;

    public string Value { get; }

    public static TaskTitle Create(string? value) =>
        new(Guard.RequiredText(value, MaxLength, "Task title"));

    public override string ToString() => Value;
}
