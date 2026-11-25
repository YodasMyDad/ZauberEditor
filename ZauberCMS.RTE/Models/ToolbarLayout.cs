using System.Text.Json;

namespace ZauberCMS.RTE.Models;

/// <summary>The groups, items and separators shown in the toolbar, in order.</summary>
/// <param name="Entries">The toolbar entries.</param>
public sealed record ToolbarLayout(params IReadOnlyList<ToolbarLayoutEntry> Entries)
{
    /// <summary>The toolbar entries.</summary>
    public IReadOnlyList<ToolbarLayoutEntry> Entries { get; init => field = LayoutGuard.NoNulls(value, nameof(Entries)); } = LayoutGuard.NoNulls(Entries, nameof(Entries));

    /// <summary>Basic formatting and alignment.</summary>
    public static ToolbarLayout Simple { get; } = new(
        new ToolbarGroup(ToolbarItemIds.Bold, ToolbarItemIds.Italic, ToolbarItemIds.Underline),
        new ToolbarSeparator(),
        new ToolbarGroup(ToolbarItemIds.ClearFormatting),
        new ToolbarSeparator(),
        new ToolbarGroup(ToolbarItemIds.AlignLeft, ToolbarItemIds.AlignCenter, ToolbarItemIds.AlignRight));

    /// <summary>Every built-in item.</summary>
    public static ToolbarLayout Full { get; } = new(
        new ToolbarGroup(
            ToolbarItemIds.Bold, ToolbarItemIds.Italic, ToolbarItemIds.Underline, ToolbarItemIds.Strike,
            ToolbarItemIds.Code, ToolbarItemIds.Subscript, ToolbarItemIds.Superscript),
        new ToolbarSeparator(),
        new ToolbarGroup(ToolbarItemIds.TextColor, ToolbarItemIds.BackgroundColor),
        new ToolbarSeparator(),
        new ToolbarGroup(ToolbarItemIds.Headings, ToolbarItemIds.Blockquote),
        new ToolbarSeparator(),
        new ToolbarGroup(ToolbarItemIds.UnorderedList, ToolbarItemIds.OrderedList, ToolbarItemIds.Outdent, ToolbarItemIds.Indent),
        new ToolbarSeparator(),
        new ToolbarGroup(
            ToolbarItemIds.Link, ToolbarItemIds.Unlink, ToolbarItemIds.Image, ToolbarItemIds.Table,
            ToolbarItemIds.HorizontalRule, ToolbarItemIds.SpecialCharacters),
        new ToolbarSeparator(),
        new ToolbarGroup(ToolbarItemIds.AlignLeft, ToolbarItemIds.AlignCenter, ToolbarItemIds.AlignRight, ToolbarItemIds.Justify),
        new ToolbarSeparator(),
        new ToolbarGroup(
            ToolbarItemIds.ClearFormatting, ToolbarItemIds.Undo, ToolbarItemIds.Redo, ToolbarItemIds.FindReplace,
            ToolbarItemIds.PastePlainText),
        new ToolbarSeparator(),
        new ToolbarGroup(ToolbarItemIds.ViewSource, ToolbarItemIds.Fullscreen, ToolbarItemIds.ThemeToggle));

    /// <summary>Whether both layouts have the same entries in the same order.</summary>
    /// <param name="other">The layout to compare with.</param>
    public bool Equals(ToolbarLayout? other) => other is not null && ValueEquality.Sequence(Entries, other.Entries);

    /// <inheritdoc />
    public override int GetHashCode() => ValueEquality.SequenceHash(Entries);

    /// <summary>Serialises the layout to JSON.</summary>
    public string ToJson() => JsonSerializer.Serialize(this, ToolbarLayoutJsonContext.Default.ToolbarLayout);

    /// <summary>Reads a layout written by <see cref="ToJson"/>.</summary>
    /// <param name="json">The JSON text.</param>
    /// <exception cref="JsonException">The JSON is not a valid toolbar layout.</exception>
    public static ToolbarLayout FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);
        try
        {
            return JsonSerializer.Deserialize(json, ToolbarLayoutJsonContext.Default.ToolbarLayout)
                ?? throw new JsonException("The JSON does not contain a toolbar layout.");
        }
        catch (ArgumentException exception)
        {
            throw new JsonException("The JSON is not a valid toolbar layout.", exception);
        }
    }
}
