using Microsoft.Extensions.Logging;

namespace ZauberCMS.RTE.Components.Internal;

internal static partial class EditorLog
{
    [LoggerMessage(EventId = 2001, Level = LogLevel.Error, Message = "Editor operation '{Operation}' failed.")]
    public static partial void OperationFailed(this ILogger logger, string operation, Exception exception);

    [LoggerMessage(EventId = 2002, Level = LogLevel.Error, Message = "The editor failed to initialise.")]
    public static partial void InitializationFailed(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 2003, Level = LogLevel.Warning, Message = "Editor script reported an error: {Message}")]
    public static partial void ScriptError(this ILogger logger, string message);

    [LoggerMessage(EventId = 2004, Level = LogLevel.Warning, Message = "The toolbar layout references items that were never registered: {ItemIds}")]
    public static partial void UnknownToolbarItems(this ILogger logger, IEnumerable<string> itemIds);

    [LoggerMessage(EventId = 2006, Level = LogLevel.Debug, Message = "The toolbar layout references removed items: {ItemIds}")]
    public static partial void RemovedToolbarItems(this ILogger logger, IEnumerable<string> itemIds);

    [LoggerMessage(EventId = 2007, Level = LogLevel.Warning, Message = "Edited content was larger than MaxContentBytes ({MaxBytes} bytes) and was not delivered.")]
    public static partial void ContentNotDelivered(this ILogger logger, long maxBytes);

    [LoggerMessage(EventId = 2005, Level = LogLevel.Information, Message = "The editor rejected input: {Message}")]
    public static partial void InputRejected(this ILogger logger, string message);
}
