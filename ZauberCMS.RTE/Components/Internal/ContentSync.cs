namespace ZauberCMS.RTE.Components.Internal;

internal sealed class ContentSync(string? initialValue)
{
    private readonly List<string> _inFlightEchoes = [];
    private string? _lastParameter = initialValue;

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

    public bool IsCurrent(int revision) => revision == Revision;

    public bool Accept(int revision, string html)
    {
        if (!IsCurrent(revision) || string.Equals(html, KnownHtml, StringComparison.Ordinal))
        {
            return false;
        }

        KnownHtml = html;
        return true;
    }

    public void BeginEcho(string html) => _inFlightEchoes.Add(html);

    public void EndEcho(string html) => _inFlightEchoes.Remove(html);
}
