namespace TaskTracker.Application.Common;

internal static class TimeProviderExtensions
{
    extension(TimeProvider timeProvider)
    {
        public DateOnly Today => DateOnly.FromDateTime(timeProvider.GetLocalNow().DateTime);
    }
}
