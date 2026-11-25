using Microsoft.Extensions.Logging;

namespace ZauberCMS.RTE.Services;

internal static partial class ToolbarLog
{
    [LoggerMessage(EventId = 3001, Level = LogLevel.Error, Message = "Toolbar item '{ItemType}' could not be created and was skipped.")]
    public static partial void ItemNotCreated(this ILogger logger, string itemType, Exception exception);
}
