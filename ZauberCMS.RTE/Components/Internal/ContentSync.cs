namespace ZauberCMS.RTE.Components.Internal;

internal sealed class ContentSync(string? initialValue)
{
    private readonly List<string> _inFlightEchoes = [];
    private string? _lastParameter = initialValue;
    private int _received;

    public string? KnownHtml { get; private set; } = initialValue;

    public int Revision { get; private set; }

    public bool ParameterChanged(string? value)
    {
        if (string.Equals(value, _lastParameter, StringComparison.Ordinal))
        {
            return false;
        }

        _lastParameter = value;
        if (value is not null && _inFlightEchoes.Contains(value))
        {
            return false;
        }

        if (string.Equals(value, KnownHtml, StringComparison.Ordinal))
        {
            return false;
        }

        KnownHtml = value;
        Revision++;
        return true;
    }

    public ContentNotification? Receive(int revision) =>
        revision == Revision ? new(revision, ++_received) : null;

    public bool Accept(ContentNotification notification, string html)
    {
        if (notification.Revision != Revision
            || notification.Sequence != _received
            || string.Equals(html, KnownHtml, StringComparison.Ordinal))
        {
            return false;
        }

        KnownHtml = html;
        return true;
    }

    public void BeginEcho(string html) => _inFlightEchoes.Add(html);

    public void EndEcho(string html) => _inFlightEchoes.Remove(html);
}
