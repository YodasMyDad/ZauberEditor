using ZauberCMS.RTE.Models;

namespace ZauberCMS.RTE.Examples;

/// <summary>
/// Examples demonstrating how to create custom dropdown toolbar items.
/// The dropdown component is fully extensible - not limited to headings!
/// 
/// NOTE: These are CONCEPTUAL examples showing the structure and extensibility.
/// The ExecuteAsync implementations use Task.CompletedTask as placeholders.
/// In a real implementation, you would:
/// - Implement actual font/color application logic
/// - Track active states properly in EditorState
/// - Use the IEditorApi methods available in your version
/// </summary>
public static class CustomDropdownExamples
{
    /// <summary>
    /// Example: Font Family Dropdown
    /// Shows different font options that can be applied to text
    /// </summary>
    public class FontFamilyDropdownItem : ToolbarItemBase
    {
        private static readonly List<IToolbarItem> _childItems =
        [
            new FontLabelItem(),
            new ArialFontItem(),
            new TimesNewRomanFontItem(),
            new CourierFontItem(),
            new GeorgiaFontItem()
        ];

        public override string Id => "fontFamily";
        public override string Label => "Font";
        public override string IconCss => "fa-font";
        public override ToolbarPlacement Placement => ToolbarPlacement.Inline;
        public override ToolbarItemType ItemType => ToolbarItemType.Dropdown;
        public override bool IsToggle => true;
        public override List<IToolbarItem> ChildItems => _childItems;

        public override bool IsActive(EditorState state) => 
            state.ActiveMarks.Any(m => m.StartsWith("font-"));

        public override Task ExecuteAsync(IEditorApi api) => Task.CompletedTask;
    }

    public class FontLabelItem : ToolbarItemBase
    {
        public override string Id => "font-default";
        public override string Label => "Default Font";
        public override string IconCss => "fa-font";
        public override ToolbarPlacement Placement => ToolbarPlacement.Inline;
        public override bool IsDropdownLabel => true;
        public override bool IsActive(EditorState state) => !state.ActiveMarks.Any(m => m.StartsWith("font-"));
        public override async Task ExecuteAsync(IEditorApi api)
        {
            // TODO: Implement font family removal logic
            await Task.CompletedTask;
        }
    }

    public class ArialFontItem : ToolbarItemBase
    {
        public override string Id => "font-arial";
        public override string Label => "Arial";
        public override string IconCss => "";
        public override ToolbarPlacement Placement => ToolbarPlacement.Inline;
        public override bool IsActive(EditorState state) => state.ActiveMarks.Contains("font-arial");
        public override async Task ExecuteAsync(IEditorApi api)
        {
            // TODO: Implement font family application logic
            // Example: await api.InsertHtmlAsync("<span style='font-family:Arial'>text</span>");
            await Task.CompletedTask;
        }
    }

    public class TimesNewRomanFontItem : ToolbarItemBase
    {
        public override string Id => "font-times";
        public override string Label => "Times New Roman";
        public override string IconCss => "";
        public override ToolbarPlacement Placement => ToolbarPlacement.Inline;
        public override bool IsActive(EditorState state) => state.ActiveMarks.Contains("font-times");
        public override async Task ExecuteAsync(IEditorApi api)
        {
            // TODO: Implement font family application logic
            await Task.CompletedTask;
        }
    }

    public class CourierFontItem : ToolbarItemBase
    {
        public override string Id => "font-courier";
        public override string Label => "Courier";
        public override string IconCss => "";
        public override ToolbarPlacement Placement => ToolbarPlacement.Inline;
        public override bool IsActive(EditorState state) => state.ActiveMarks.Contains("font-courier");
        public override async Task ExecuteAsync(IEditorApi api)
        {
            // TODO: Implement font family application logic
            await Task.CompletedTask;
        }
    }

    public class GeorgiaFontItem : ToolbarItemBase
    {
        public override string Id => "font-georgia";
        public override string Label => "Georgia";
        public override string IconCss => "";
        public override ToolbarPlacement Placement => ToolbarPlacement.Inline;
        public override bool IsActive(EditorState state) => state.ActiveMarks.Contains("font-georgia");
        public override async Task ExecuteAsync(IEditorApi api)
        {
            // TODO: Implement font family application logic
            await Task.CompletedTask;
        }
    }

    /// <summary>
    /// Example: Text Color Dropdown
    /// Shows color options with icons
    /// </summary>
    public class TextColorDropdownItem : ToolbarItemBase
    {
        private static readonly List<IToolbarItem> _childItems =
        [
            new ColorDefaultItem(),
            new ColorRedItem(),
            new ColorBlueItem(),
            new ColorGreenItem(),
            new ColorOrangeItem()
        ];

        public override string Id => "textColor";
        public override string Label => "Color";
        public override string IconCss => "fa-palette";
        public override ToolbarPlacement Placement => ToolbarPlacement.Inline;
        public override ToolbarItemType ItemType => ToolbarItemType.Dropdown;
        public override List<IToolbarItem> ChildItems => _childItems;

        public override Task ExecuteAsync(IEditorApi api) => Task.CompletedTask;
    }

    public class ColorDefaultItem : ToolbarItemBase
    {
        public override string Id => "color-default";
        public override string Label => "Default";
        public override string IconCss => "fa-font";
        public override ToolbarPlacement Placement => ToolbarPlacement.Inline;
        public override bool IsDropdownLabel => true;
        public override async Task ExecuteAsync(IEditorApi api)
        {
            // TODO: Implement color removal logic
            await Task.CompletedTask;
        }
    }

    public class ColorRedItem : ToolbarItemBase
    {
        public override string Id => "color-red";
        public override string Label => "Red";
        public override string IconCss => "fa-square"; // Will be styled red via CSS
        public override ToolbarPlacement Placement => ToolbarPlacement.Inline;
        public override async Task ExecuteAsync(IEditorApi api)
        {
            // TODO: Implement color application logic
            await Task.CompletedTask;
        }
    }

    public class ColorBlueItem : ToolbarItemBase
    {
        public override string Id => "color-blue";
        public override string Label => "Blue";
        public override string IconCss => "fa-square";
        public override ToolbarPlacement Placement => ToolbarPlacement.Inline;
        public override async Task ExecuteAsync(IEditorApi api)
        {
            // TODO: Implement color application logic
            await Task.CompletedTask;
        }
    }

    public class ColorGreenItem : ToolbarItemBase
    {
        public override string Id => "color-green";
        public override string Label => "Green";
        public override string IconCss => "fa-square";
        public override ToolbarPlacement Placement => ToolbarPlacement.Inline;
        public override async Task ExecuteAsync(IEditorApi api)
        {
            // TODO: Implement color application logic
            await Task.CompletedTask;
        }
    }

    public class ColorOrangeItem : ToolbarItemBase
    {
        public override string Id => "color-orange";
        public override string Label => "Orange";
        public override string IconCss => "fa-square";
        public override ToolbarPlacement Placement => ToolbarPlacement.Inline;
        public override async Task ExecuteAsync(IEditorApi api)
        {
            // TODO: Implement color application logic
            await Task.CompletedTask;
        }
    }

    /// <summary>
    /// CSS to style the custom dropdowns (add to your custom stylesheet)
    /// </summary>
    public const string CustomDropdownCss = @"
/* Style font dropdown items with their actual fonts */
.rte-dropdown-font-arial .rte-dropdown-item-label {
    font-family: Arial, sans-serif !important;
}

.rte-dropdown-font-times .rte-dropdown-item-label {
    font-family: 'Times New Roman', serif !important;
}

.rte-dropdown-font-courier .rte-dropdown-item-label {
    font-family: 'Courier New', monospace !important;
}

.rte-dropdown-font-georgia .rte-dropdown-item-label {
    font-family: Georgia, serif !important;
}

/* Style color dropdown icons with actual colors */
.rte-dropdown-color-red i.fa-square {
    color: #dc2626 !important;
}

.rte-dropdown-color-blue i.fa-square {
    color: #2563eb !important;
}

.rte-dropdown-color-green i.fa-square {
    color: #16a34a !important;
}

.rte-dropdown-color-orange i.fa-square {
    color: #ea580c !important;
}
";

    /// <summary>
    /// Example layout using custom dropdowns
    /// </summary>
    public static ToolbarLayout CustomDropdownLayout => ToolbarLayout.FromItems(
        new ToolbarBlock("bold", "italic", "underline"),
        new ToolbarSeparator(),
        new ToolbarBlock("headings"),       // Built-in headings dropdown
        new ToolbarSeparator(),
        new ToolbarBlock("fontFamily"),     // Custom font dropdown
        new ToolbarSeparator(),
        new ToolbarBlock("textColor"),      // Custom color dropdown
        new ToolbarSeparator(),
        new ToolbarBlock("link", "image")
    );
}

/// <summary>
/// Usage instructions for custom dropdowns
/// </summary>
public class CustomDropdownUsage
{
    /*
     * HOW TO USE CUSTOM DROPDOWNS:
     * 
     * 1. Create your dropdown item (parent):
     *    - Set ItemType = ToolbarItemType.Dropdown
     *    - Provide ChildItems list
     *    - Choose an icon for the dropdown trigger
     * 
     * 2. Create child items:
     *    - First item can be IsDropdownLabel = true (resets to default)
     *    - Each item needs unique Id
     *    - Set IconCss = "" to show text label instead of icon
     *    - Implement IsActive() to show selection state
     * 
     * 3. Style with CSS (optional):
     *    - Each dropdown item gets class: rte-dropdown-{itemId}
     *    - Style the .rte-dropdown-item-label span for custom appearance
     *    - Example: .rte-dropdown-font-arial .rte-dropdown-item-label { font-family: Arial; }
     * 
     * 4. Add to toolbar:
     *    var layout = ToolbarLayout.FromItems(
     *        new ToolbarBlock("bold", "italic"),
     *        new ToolbarSeparator(),
     *        new ToolbarBlock("fontFamily", "textColor")  // Your custom dropdowns
     *    );
     * 
     * KEY POINTS:
     * - No hardcoded logic - fully extensible
     * - Works for ANY custom functionality (fonts, colors, emojis, etc.)
     * - Child items render with label + optional icon
     * - Active state automatically shows checkmark
     * - CSS classes allow complete visual customization
     */
}

