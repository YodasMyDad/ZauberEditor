using Microsoft.Extensions.Logging;
using NSubstitute.Core;

namespace ZauberCMS.RTE.Tests.TestSupport;

internal static class SubstituteCalls
{
    public static ICall[] To(object substitute, string methodName) =>
        [.. substitute.ReceivedCalls().Where(call => call.GetMethodInfo().Name == methodName)];

    public static ICall[] Commands(object substitute) =>
        [.. substitute.ReceivedCalls().Where(call => call.GetMethodInfo().ReturnType.IsAssignableTo(typeof(Task)))];

    public static (LogLevel Level, string Message)[] Logs(ILogger logger) =>
        [.. To(logger, nameof(ILogger.Log)).Select(call => ((LogLevel)call.GetArguments()[0]!, call.GetArguments()[2]?.ToString() ?? string.Empty))];
}
