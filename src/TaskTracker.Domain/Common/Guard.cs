namespace TaskTracker.Domain.Common;

internal static class Guard
{
    public static string RequiredText(string? value, int maxLength, string fieldName)
    {
        var trimmed = value?.Trim();

        if (string.IsNullOrEmpty(trimmed))
        {
            throw new DomainException($"{fieldName} is required.");
        }

        if (trimmed.Length > maxLength)
        {
            throw new DomainException($"{fieldName} cannot be longer than {maxLength} characters.");
        }

        return trimmed;
    }
}
