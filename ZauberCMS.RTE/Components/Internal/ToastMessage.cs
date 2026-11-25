using ZauberCMS.RTE.Models;

namespace ZauberCMS.RTE.Components.Internal;

internal sealed record ToastMessage(int Id, string Message, ToastType Type, ITimer Timer);
