using System.ComponentModel;

namespace ZauberCMS.RTE.Components.Internal;

/// <summary>Inserts a special character. Opened by the built-in special characters item; not for direct use.</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed partial class SpecialCharactersPanel
{
    private static readonly IReadOnlyList<SpecialCharacter> Characters =
    [
        new("©", "Copyright sign"),
        new("®", "Registered sign"),
        new("™", "Trade mark sign"),
        new("€", "Euro sign"),
        new("£", "Pound sign"),
        new("¥", "Yen sign"),
        new("°", "Degree sign"),
        new("±", "Plus-minus sign"),
        new("×", "Multiplication sign"),
        new("÷", "Division sign"),
        new("–", "En dash"),
        new("—", "Em dash"),
        new("…", "Ellipsis"),
        new("«", "Left double angle quotation mark"),
        new("»", "Right double angle quotation mark"),
        new("‘", "Left single quotation mark"),
        new("’", "Right single quotation mark"),
        new("“", "Left double quotation mark"),
        new("”", "Right double quotation mark"),
        new("§", "Section sign"),
        new("¶", "Pilcrow sign"),
        new("•", "Bullet"),
        new("†", "Dagger"),
        new("½", "One half"),
        new("¼", "One quarter"),
        new("¾", "Three quarters"),
        new("→", "Rightwards arrow"),
        new("←", "Leftwards arrow"),
        new(" ", "Non-breaking space", "nbsp")
    ];

    /// <inheritdoc />
    public override string Title => "Special character";

    private static string CellClass(SpecialCharacter character) =>
        character.Display is null ? "rte-char-btn" : "rte-char-btn rte-char-btn-named";

    private async Task InsertAsync(SpecialCharacter character)
    {
        await Api.InsertTextAsync(character.Text);
        await CloseAsync();
    }
}
