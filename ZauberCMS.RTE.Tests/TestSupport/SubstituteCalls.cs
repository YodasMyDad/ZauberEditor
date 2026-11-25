using NSubstitute.Core;

namespace ZauberCMS.RTE.Tests.TestSupport;

internal static class SubstituteCalls
{
    public static ICall[] To(object substitute, string methodName) =>
        [.. substitute.ReceivedCalls().Where(call => call.GetMethodInfo().Name == methodName)];

    public static ICall[] Commands(object substitute) =>
        [.. substitute.ReceivedCalls().Where(call => call.GetMethodInfo().ReturnType.IsAssignableTo(typeof(Task)))];
}
