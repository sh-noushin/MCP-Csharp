namespace TaskTracker.Domain.Tests;

internal static class TestClock
{
    public static readonly DateTimeOffset Now = new(2026, 10, 12, 9, 30, 0, TimeSpan.Zero);

    public static readonly DateOnly Today = DateOnly.FromDateTime(Now.DateTime);
}
