using System.Text;

namespace ZauberCMS.RTE.Components.Internal;

internal sealed class ContentChunks
{
    private readonly StringBuilder _buffer = new();
    private long _bytes;
    private bool _overflow;

    public void Append(string chunk, long maxBytes)
    {
        _bytes += Encoding.UTF8.GetByteCount(chunk);
        _overflow |= _bytes > maxBytes;
        if (_overflow)
        {
            _buffer.Clear();
        }
        else
        {
            _buffer.Append(chunk);
        }
    }

    public string? Complete(string? last, long maxBytes)
    {
        if (last is not null)
        {
            Append(last, maxBytes);
        }

        var html = last is null || _overflow ? null : _buffer.ToString();
        _buffer.Clear();
        _bytes = 0;
        _overflow = false;
        return html;
    }
}
