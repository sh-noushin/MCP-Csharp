using ModelContextProtocol;
using TaskTracker.Application.Common;
using TaskTracker.Domain.Common;

namespace TaskTracker.McpServer.Tools;

internal static class ToolExecution
{
    /// <summary>Surfaces business-rule failures to the client; other exceptions stay generic.</summary>
    public static async Task<TResult> RunAsync<TResult>(Func<Task<TResult>> useCase)
    {
        try
        {
            return await useCase();
        }
        catch (Exception exception) when (exception is DomainException or NotFoundException)
        {
            throw new McpException(exception.Message, exception);
        }
    }
}
