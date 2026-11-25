using Microsoft.Extensions.Logging;

namespace ZauberCMS.RTE.Interop;

internal static partial class InteropLog
{
    [LoggerMessage(EventId = 1001, Level = LogLevel.Error, Message = "Editor script call '{Identifier}' failed.")]
    public static partial void InvocationFailed(this ILogger logger, string identifier, Exception exception);

    [LoggerMessage(EventId = 1002, Level = LogLevel.Warning, Message = "Editor content is {Length} bytes, more than the {MaxBytes} byte limit; it was not transferred.")]
    public static partial void ContentTooLarge(this ILogger logger, long length, long maxBytes);

    [LoggerMessage(EventId = 1003, Level = LogLevel.Warning, Message = "Disposing the editor script instance failed.")]
    public static partial void DisposeFailed(this ILogger logger, Exception exception);
}
