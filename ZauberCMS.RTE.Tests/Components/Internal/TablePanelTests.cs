using System.Text.RegularExpressions;
using Bunit;
using ZauberCMS.RTE.Components.Internal;
using ZauberCMS.RTE.Models;

namespace ZauberCMS.RTE.Tests.Components.Internal;

public sealed class TablePanelTests : BunitContext
{
    private readonly IEditorApi _api = Substitute.For<IEditorApi>();

    [Fact]
    public void BuildsBodyRowsWithEmptyEditableCells() =>
        TablePanel.BuildTable(2, 3, hasHeader: false).ShouldBe(
            "<table class=\"rte-table\"><tbody>"
            + "<tr><td><br></td><td><br></td><td><br></td></tr>"
            + "<tr><td><br></td><td><br></td><td><br></td></tr>"
            + "</tbody></table>");

    [Fact]
    public void HeaderRowIsAddedInAHeadSection() =>
        TablePanel.BuildTable(1, 2, hasHeader: true).ShouldBe(
            "<table class=\"rte-table\"><thead><tr><th><br></th><th><br></th></tr></thead>"
            + "<tbody><tr><td><br></td><td><br></td></tr></tbody></table>");

    [Theory]
    [InlineData(0, 0, 1, 1)]
    [InlineData(-5, 4, 1, 4)]
    [InlineData(50, 100, 20, 20)]
    [InlineData(int.MaxValue, int.MinValue, 20, 1)]
    public void SizeIsClampedToTheAllowedRange(int rows, int columns, int expectedRows, int expectedColumns)
    {
        var html = TablePanel.BuildTable(rows, columns, hasHeader: false);

        Regex.Count(html, "<tr>").ShouldBe(expectedRows);
        Regex.Count(html, "<td>").ShouldBe(expectedRows * expectedColumns);
    }

    [Fact]
    public async Task InsertAddsTheTableAndClosesThePanel()
    {
        var cut = Render<TablePanel>(parameters => parameters.AddCascadingValue(_api));

        cut.Find("input[type=checkbox]").Change(true);
        cut.FindAll("input[type=number]")[0].Change("100");
        await cut.Find(".rte-panel-actions .rte-btn-primary").ClickAsync(new());

        await _api.Received(1).InsertHtmlAsync(TablePanel.BuildTable(20, 3, hasHeader: true), Arg.Any<CancellationToken>());
        await _api.Received(1).ClosePanelAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SubmittingTheFormInsertsTheTable()
    {
        var cut = Render<TablePanel>(parameters => parameters.AddCascadingValue(_api));

        await cut.Find("form.rte-panel-content").SubmitAsync();

        await _api.Received(1).InsertHtmlAsync(TablePanel.BuildTable(3, 3, hasHeader: false), Arg.Any<CancellationToken>());
        await _api.Received(1).ClosePanelAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CancelClosesWithoutInserting()
    {
        var cut = Render<TablePanel>(parameters => parameters.AddCascadingValue(_api));

        await cut.Find(".rte-panel-actions .rte-btn-secondary").ClickAsync(new());

        await _api.DidNotReceive().InsertHtmlAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _api.Received(1).ClosePanelAsync(Arg.Any<CancellationToken>());
    }
}
