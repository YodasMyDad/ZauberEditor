using System.ComponentModel;
using ZauberCMS.RTE.Models;

namespace ZauberCMS.RTE.Components.Internal;

/// <summary>Inserts a table. Opened by the built-in table item; not for direct use.</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed partial class TablePanel
{
    private const int MaxSize = 20;

    private int _rows = 3;
    private int _columns = 3;
    private bool _hasHeader;

    /// <inheritdoc />
    public override string Title => "Insert table";

    internal static string BuildTable(int rows, int columns, bool hasHeader)
    {
        rows = Math.Clamp(rows, 1, MaxSize);
        columns = Math.Clamp(columns, 1, MaxSize);
        var table = HtmlBuilder.Element("table").Class("rte-table");
        if (hasHeader)
        {
            table.Child(HtmlBuilder.Element("thead").Child(Row("th", columns)));
        }

        var body = HtmlBuilder.Element("tbody");
        for (var row = 0; row < rows; row++)
        {
            body.Child(Row("td", columns));
        }

        return table.Child(body).Build();
    }

    private static HtmlBuilder Row(string cellTag, int columns)
    {
        var row = HtmlBuilder.Element("tr");
        for (var column = 0; column < columns; column++)
        {
            row.Child(HtmlBuilder.Element(cellTag).Child(HtmlBuilder.Element("br")));
        }

        return row;
    }

    private async Task InsertAsync()
    {
        await Api.InsertHtmlAsync(BuildTable(_rows, _columns, _hasHeader));
        await CloseAsync();
    }
}
