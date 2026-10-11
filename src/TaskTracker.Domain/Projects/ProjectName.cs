using TaskTracker.Domain.Common;

namespace TaskTracker.Domain.Projects;

public sealed record ProjectName
{
    public const int MaxLength = 100;

    private ProjectName(string value) => Value = value;

    public string Value { get; }

    public static ProjectName Create(string? value) =>
        new(Guard.RequiredText(value, MaxLength, "Project name"));

    public override string ToString() => Value;
}
