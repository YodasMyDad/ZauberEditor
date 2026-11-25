using System.Globalization;
using System.Text;

namespace ZauberRTE.Web.Client.Components;

public static class ByteSize
{
    public static int Of(string? text) => Encoding.UTF8.GetByteCount(text ?? string.Empty);

    public static string Format(string? text) => Of(text) switch
    {
        < 1024 and var bytes => string.Create(CultureInfo.InvariantCulture, $"{bytes} bytes"),
        var bytes => string.Create(CultureInfo.InvariantCulture, $"{bytes / 1024d:N1} KB ({bytes:N0} bytes)")
    };
}
