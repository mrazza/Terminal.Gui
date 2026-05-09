using Terminal.Gui.Drawing;
using Terminal.Gui.ViewBase;

namespace Terminal.Gui.Views;

/// <summary>
/// A view that renders an image. Uses Sixel graphics if supported by the terminal, 
/// otherwise falls back to block character rendering.
/// </summary>
public class ImageView : View
{
    /// <summary>
    /// Gets or sets the image source as a 2D array of colors.
    /// </summary>
    public Color[,]? Source { 
        get; 
        set {
            if (field == value) return;
            field = value;
            _cachedSixelData = null;
            _sixelToRender = null;
            SetNeedsDraw();
        }
    }

    /// <summary>
    /// Gets the <see cref="SixelEncoder"/> used for Sixel rendering.
    /// </summary>
    public SixelEncoder SixelEncoder { get; } = new ();

    private SixelToRender? _sixelToRender;
    private string? _cachedSixelData;
    private Size _cachedSixelSize;
    private Size _cachedResolution;

    /// <summary>
    /// Initializes a new instance of the <see cref="ImageView"/> class.
    /// </summary>
    public ImageView()
    {
        CanFocus = true;
    }

    /// <inheritdoc/>
    protected override bool OnDrawingContent(DrawContext? context)
    {
        if (Source == null || Source.GetLength(0) == 0 || Source.GetLength(1) == 0)
        {
            return base.OnDrawingContent(context);
        }

        var driver = Application.Driver;
        var output = driver?.GetOutput();
        
        if (output != null && output.SixelSupport.IsSupported)
        {
            Size resolution = output.SixelSupport.Resolution;

            if (RenderSixel(resolution))
            {
                context?.AddDrawnRectangle(ViewportToScreen(Viewport));
                return true;
            }
        }

        // Fallback to block rendering
        return RenderFallback(context);
    }

    private bool RenderSixel(Size resolution)
    {
        var viewportPixels = new Size(Viewport.Width * resolution.Width, Viewport.Height * resolution.Height);
        
        if (_cachedSixelData == null || _cachedSixelSize != Viewport.Size || _cachedResolution != resolution)
        {
            var resized = ImageResizer.Resize(Source, viewportPixels.Width, viewportPixels.Height);
            _cachedSixelData = SixelEncoder.EncodeSixel(resized);
            _cachedSixelSize = Viewport.Size;
            _cachedResolution = resolution;
            
            _sixelToRender = new SixelToRender
            {
                Id = Id?.ToString() ?? GetHashCode().ToString(),
                SixelData = _cachedSixelData
            };
        }

        Point screenPos = ViewportToScreen(Viewport).Location;
        if (_sixelToRender!.ScreenPosition != screenPos)
        {
            _sixelToRender.ScreenPosition = screenPos;
            _sixelToRender.IsDirty = true;
        }
        
        Application.Driver?.SixelImages.AddOrUpdate(_sixelToRender.Id!, _sixelToRender, (k, v) => _sixelToRender);
        
        return true;
    }

    private bool RenderFallback(DrawContext? context)
    {
        var resized = ImageResizer.Resize(Source, Viewport.Width, Viewport.Height);
        
        for (int y = 0; y < Viewport.Height; y++)
        {
            for (int x = 0; x < Viewport.Width; x++)
            {
                var color = resized[x, y];
                var attr = new Attribute(new Color(), color);
                SetAttribute(attr);
                AddRune(x, y, (Rune)' ');
            }
        }
        
        return true;
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing && _sixelToRender?.Id != null)
        {
            Application.Driver?.SixelImages.TryRemove(_sixelToRender.Id, out _);
        }
        base.Dispose(disposing);
    }
}