using ModelContextProtocol;

namespace TaskTracker.McpServer.Tools;

internal static class EnumArgument
{
    private static readonly char[] WordSeparators = ['_', '-', ' '];

    public static TEnum Parse<TEnum>(string value, string argumentName)
        where TEnum : struct, Enum
    {
        var normalized = string.Concat(value.Split(WordSeparators, StringSplitOptions.RemoveEmptyEntries));

        return Enum.TryParse<TEnum>(normalized, ignoreCase: true, out var parsed) && Enum.IsDefined(parsed)
            ? parsed
            : throw new McpException(
                $"'{value}' is not a valid {argumentName}. Use one of: {string.Join(", ", Enum.GetNames<TEnum>())}.");
    }
}
