using Microsoft.JSInterop;
using Microsoft.Extensions.Logging;
using Zauber.RTE.Models;

namespace Zauber.RTE.Services;

/// <summary>
/// JavaScript runtime implementation for Zauber RTE
/// </summary>
public class ZauberJsRuntime(IJSRuntime jsRuntime, ILogger<ZauberJsRuntime> logger) : IZauberJsRuntime
{
    public SelectionInterop Selection { get; } = new SelectionInteropImpl(jsRuntime, logger);
    public ClipboardInterop Clipboard { get; } = new ClipboardInteropImpl(jsRuntime, logger);
    public ImageInterop Image { get; } = new ImageInteropImpl(jsRuntime, logger);
    public MutationInterop Mutation { get; } = new MutationInteropImpl(jsRuntime, logger);
    public PanelInterop Panel { get; } = new PanelInteropImpl(jsRuntime, logger);
    public HistoryInterop History { get; } = new HistoryInteropImpl(jsRuntime, logger);
}

/// <summary>
/// Selection interop implementation
/// </summary>
internal class SelectionInteropImpl(IJSRuntime jsRuntime, ILogger logger) : SelectionInterop
{
    public async Task<SelectionInfo?> GetSelectionAsync(string editorId)
    {
        try
        {
            return await jsRuntime.InvokeAsync<SelectionInfo?>("ZauberRTE.selection.get", editorId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get selection for editor {EditorId}", editorId);
            return null;
        }
    }

    public async Task SetSelectionAsync(string editorId, int startOffset, int endOffset)
    {
        try
        {
            await jsRuntime.InvokeVoidAsync("ZauberRTE.selection.set", editorId, startOffset, endOffset);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to set selection for editor {EditorId}", editorId);
        }
    }

    public async Task CollapseSelectionAsync(string editorId, bool toStart = false)
    {
        try
        {
            await jsRuntime.InvokeVoidAsync("ZauberRTE.selection.collapse", editorId, toStart);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to collapse selection for editor {EditorId}", editorId);
        }
    }

    public async Task WrapSelectionAsync(string editorId, string tagName, Dictionary<string, string>? attributes = null)
    {
        try
        {
            await jsRuntime.InvokeVoidAsync("ZauberRTE.selection.wrap", editorId, tagName, attributes);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to wrap selection for editor {EditorId}", editorId);
        }
    }

    public async Task UnwrapSelectionAsync(string editorId, string tagName)
    {
        try
        {
            await jsRuntime.InvokeVoidAsync("ZauberRTE.selection.unwrap", editorId, tagName);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to unwrap selection for editor {EditorId}", editorId);
        }
    }

    public async Task InsertHtmlAsync(string editorId, string html)
    {
        try
        {
            await jsRuntime.InvokeVoidAsync("ZauberRTE.selection.insertHtml", editorId, html);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to insert HTML for editor {EditorId}", editorId);
        }
    }

    public async Task<HashSet<string>> GetActiveMarksAsync(string editorId)
    {
        try
        {
            var marks = await jsRuntime.InvokeAsync<string[]>("ZauberRTE.selection.getActiveMarks", editorId);
            return new HashSet<string>(marks ?? Array.Empty<string>());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get active marks for editor {EditorId}", editorId);
            return new HashSet<string>();
        }
    }

    public async Task<string?> GetCurrentBlockTypeAsync(string editorId)
    {
        try
        {
            return await jsRuntime.InvokeAsync<string>("ZauberRTE.selection.getCurrentBlockType", editorId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get current block type for editor {EditorId}", editorId);
            return null;
        }
    }

    public async Task ToggleMarkAsync(string editorId, string markName)
    {
        logger.LogInformation("C#: ToggleMarkAsync called with editorId={EditorId}, markName={MarkName}", editorId, markName);
        try
        {
            logger.LogInformation("C#: About to call InvokeVoidAsync");
            await jsRuntime.InvokeVoidAsync("ZauberRTE.selection.toggleMark", editorId, markName);
            logger.LogInformation("C#: InvokeVoidAsync completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to toggle mark {MarkName} for editor {EditorId}", markName, editorId);
        }
    }

    public async Task SetBlockTypeAsync(string editorId, string blockType, Dictionary<string, string>? attributes = null)
    {
        try
        {
            await jsRuntime.InvokeVoidAsync("ZauberRTE.selection.setBlockType", editorId, blockType, attributes);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to set block type {BlockType} for editor {EditorId}", blockType, editorId);
        }
    }

    public async Task SetBlockStyleAsync(string editorId, Dictionary<string, string> styles)
    {
        try
        {
            await jsRuntime.InvokeVoidAsync("ZauberRTE.selection.setBlockStyle", editorId, styles);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to set block style for editor {EditorId}", editorId);
        }
    }

    public async Task<int?> GetCurrentHeadingLevelAsync(string editorId)
    {
        try
        {
            var result = await jsRuntime.InvokeAsync<int>("ZauberRTE.selection.getCurrentHeadingLevel", editorId);
            return result > 0 ? result : null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get current heading level for editor {EditorId}", editorId);
            return null;
        }
    }

    public async Task ClearFormattingAsync(string editorId)
    {
        try
        {
            await jsRuntime.InvokeVoidAsync("ZauberRTE.selection.clearFormatting", editorId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to clear formatting for editor {EditorId}", editorId);
        }
    }
}

/// <summary>
/// Clipboard interop implementation
/// </summary>
internal class ClipboardInteropImpl(IJSRuntime jsRuntime, ILogger logger) : ClipboardInterop
{
    public async Task<string?> GetClipboardHtmlAsync()
    {
        try
        {
            return await jsRuntime.InvokeAsync<string>("ZauberRTE.clipboard.getHtml");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get clipboard HTML");
            return null;
        }
    }

    public async Task<string> CleanHtmlAsync(string html, HtmlPolicy policy)
    {
        try
        {
            return await jsRuntime.InvokeAsync<string>("ZauberRTE.clipboard.cleanHtml", html, policy);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to clean HTML");
            return html; // Return original if cleaning fails
        }
    }

    public async Task InsertCleanedHtmlAsync(string editorId, string html)
    {
        try
        {
            await jsRuntime.InvokeVoidAsync("ZauberRTE.clipboard.insertCleanedHtml", editorId, html);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to insert cleaned HTML for editor {EditorId}", editorId);
        }
    }
}

/// <summary>
/// Image interop implementation
/// </summary>
internal class ImageInteropImpl(IJSRuntime jsRuntime, ILogger logger) : ImageInterop
{
    public async Task ShowResizeHandlesAsync(string editorId, object imageId)
    {
        try
        {
            await jsRuntime.InvokeVoidAsync("ZauberRTE.image.showResizeHandles", editorId, imageId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to show resize handles for editor {EditorId}", editorId);
        }
    }

    public async Task HideResizeHandlesAsync(string editorId)
    {
        try
        {
            await jsRuntime.InvokeVoidAsync("ZauberRTE.image.hideResizeHandles", editorId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to hide resize handles for editor {EditorId}", editorId);
        }
    }

    public async Task ResizeImageAsync(string editorId, string imageSelector, int width, int height, bool maintainAspectRatio = true)
    {
        try
        {
            await jsRuntime.InvokeVoidAsync("ZauberRTE.image.resize", editorId, imageSelector, width, height, maintainAspectRatio);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to resize image for editor {EditorId}", editorId);
        }
    }

    public async Task<(int width, int height)> GetImageDimensionsAsync(string imageSrc)
    {
        try
        {
            var result = await jsRuntime.InvokeAsync<int[]>("ZauberRTE.image.getDimensions", imageSrc);
            return result != null && result.Length == 2 ? (result[0], result[1]) : (0, 0);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get image dimensions for {ImageSrc}", imageSrc);
            return (0, 0);
        }
    }

    public async Task<bool> ToggleAspectRatioAsync()
    {
        try
        {
            return await jsRuntime.InvokeAsync<bool>("ZauberRTE.image.toggleAspectRatio");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to toggle aspect ratio");
            return true; // Default to maintaining aspect ratio
        }
    }
}

/// <summary>
/// Mutation interop implementation
/// </summary>
internal class MutationInteropImpl(IJSRuntime jsRuntime, ILogger logger) : MutationInterop
{
    public async Task StartObservingAsync(string editorId)
    {
        try
        {
            await jsRuntime.InvokeVoidAsync("ZauberRTE.mutation.startObserving", editorId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to start observing mutations for editor {EditorId}", editorId);
        }
    }

    public async Task StopObservingAsync(string editorId)
    {
        try
        {
            await jsRuntime.InvokeVoidAsync("ZauberRTE.mutation.stopObserving", editorId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to stop observing mutations for editor {EditorId}", editorId);
        }
    }

    public async Task<string?> GetHtmlAsync(string editorId)
    {
        try
        {
            return await jsRuntime.InvokeAsync<string>("ZauberRTE.mutation.getHtml", editorId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get HTML for editor {EditorId}", editorId);
            return null;
        }
    }

    public async Task SetHtmlAsync(string editorId, string html)
    {
        try
        {
            await jsRuntime.InvokeVoidAsync("ZauberRTE.mutation.setHtml", editorId, html);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to set HTML for editor {EditorId}", editorId);
        }
    }

    public async Task<string> SerializeHtmlAsync(string editorId)
    {
        try
        {
            return await jsRuntime.InvokeAsync<string>("ZauberRTE.mutation.serializeHtml", editorId) ?? string.Empty;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to serialize HTML for editor {EditorId}", editorId);
            return string.Empty;
        }
    }
}

/// <summary>
/// Panel interop implementation
/// </summary>
internal class PanelInteropImpl(IJSRuntime jsRuntime, ILogger logger) : PanelInterop
{
    public async Task OpenPanelAsync(string editorId, string panelId)
    {
        try
        {
            await jsRuntime.InvokeVoidAsync("ZauberRTE.panel.open", editorId, panelId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to open panel {PanelId} for editor {EditorId}", panelId, editorId);
        }
    }

    public async Task ClosePanelAsync(string editorId, string panelId)
    {
        try
        {
            await jsRuntime.InvokeVoidAsync("ZauberRTE.panel.close", editorId, panelId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to close panel {PanelId} for editor {EditorId}", panelId, editorId);
        }
    }

    public async Task TrapFocusAsync(string panelId)
    {
        try
        {
            await jsRuntime.InvokeVoidAsync("ZauberRTE.panel.trapFocus", panelId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to trap focus for panel {PanelId}", panelId);
        }
    }

    public async Task ReleaseFocusAsync(string panelId)
    {
        try
        {
            await jsRuntime.InvokeVoidAsync("ZauberRTE.panel.releaseFocus", panelId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to release focus for panel {PanelId}", panelId);
        }
    }
}

/// <summary>
/// History interop implementation
/// </summary>
internal class HistoryInteropImpl(IJSRuntime jsRuntime, ILogger logger) : HistoryInterop
{
    public async Task RecordStateAsync(string editorId)
    {
        try
        {
            await jsRuntime.InvokeVoidAsync("ZauberRTE.history.recordState", editorId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to record state for editor {EditorId}", editorId);
        }
    }

    public async Task<bool> UndoAsync(string editorId)
    {
        try
        {
            return await jsRuntime.InvokeAsync<bool>("ZauberRTE.history.undo", editorId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to undo for editor {EditorId}", editorId);
            return false;
        }
    }

    public async Task<bool> RedoAsync(string editorId)
    {
        try
        {
            return await jsRuntime.InvokeAsync<bool>("ZauberRTE.history.redo", editorId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to redo for editor {EditorId}", editorId);
            return false;
        }
    }

    public async Task<bool> CanUndoAsync(string editorId)
    {
        try
        {
            return await jsRuntime.InvokeAsync<bool>("ZauberRTE.history.canUndo", editorId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to check can undo for editor {EditorId}", editorId);
            return false;
        }
    }

    public async Task<bool> CanRedoAsync(string editorId)
    {
        try
        {
            return await jsRuntime.InvokeAsync<bool>("ZauberRTE.history.canRedo", editorId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to check can redo for editor {EditorId}", editorId);
            return false;
        }
    }

    public async Task ClearHistoryAsync(string editorId)
    {
        try
        {
            await jsRuntime.InvokeVoidAsync("ZauberRTE.history.clearHistory", editorId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to clear history for editor {EditorId}", editorId);
        }
    }
}
