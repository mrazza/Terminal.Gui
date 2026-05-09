using System.Collections.Concurrent;

namespace Terminal.Gui.Drawing;

/// <summary>
///     Translates colors in an image into a Palette of up to <see cref="MaxColors"/> colors (typically 256).
/// </summary>
public class ColorQuantizer
{
    /// <summary>
    ///     Gets the current colors in the palette based on the last call to
    ///     <see cref="BuildPalette"/>.
    /// </summary>
    public IReadOnlyCollection<Color> Palette { get; private set; } = new List<Color> ();

    /// <summary>
    ///     Gets or sets the maximum number of colors to put into the <see cref="Palette"/>.
    ///     Defaults to 256 (the maximum for sixel images).
    /// </summary>
    public int MaxColors { get; set; } = 256;

    /// <summary>
    ///     Gets or sets the algorithm used to map novel colors into existing
    ///     palette colors (closest match). Defaults to <see cref="EuclideanColorDistance"/>
    /// </summary>
    public IColorDistance DistanceAlgorithm { get; set; } = new EuclideanColorDistance ();

    /// <summary>
    ///     Gets or sets the algorithm used to build the <see cref="Palette"/>.
    /// </summary>
    public IPaletteBuilder PaletteBuildingAlgorithm { get; set; } = new PopularityPaletteWithThreshold (new EuclideanColorDistance (), 8);

    private readonly ConcurrentDictionary<Color, int> _nearestColorCache = new ();

    /// <summary>
    ///     Builds a <see cref="Palette"/> of colors that most represent the colors used in <paramref name="pixels"/> image.
    ///     This is based on the currently configured <see cref="PaletteBuildingAlgorithm"/>.
    /// </summary>
    /// <param name="pixels"></param>
    public void BuildPalette (Color [,] pixels)
    {
        int width = pixels.GetLength (0);
        int height = pixels.GetLength (1);

        // Optimization: If the total number of pixels is large, sample the image instead of using all colors
        // to build the palette. This prevents O(N*M) explosions in palette builders.
        const int maxSamplePixels = 10000;
        List<Color> sampleColors = [];
        
        int totalPixels = width * height;
        int step = Math.Max(1, totalPixels / maxSamplePixels);

        for (int i = 0; i < totalPixels; i += step)
        {
            int x = i % width;
            int y = i / width;
            if (y < height)
            {
                sampleColors.Add(pixels[x, y]);
            }
        }

        _nearestColorCache.Clear ();
        Palette = PaletteBuildingAlgorithm.BuildPalette (sampleColors, MaxColors);
    }

    /// <summary>
    ///     Returns the closest color in <see cref="Palette"/> that matches <paramref name="toTranslate"/>
    ///     based on the color comparison algorithm defined by <see cref="DistanceAlgorithm"/>
    /// </summary>
    /// <param name="toTranslate"></param>
    /// <returns></returns>
    public int GetNearestColor (Color toTranslate)
    {
        if (_nearestColorCache.TryGetValue (toTranslate, out int cachedAnswer))
        {
            return cachedAnswer;
        }

        // Simple nearest color matching based on DistanceAlgorithm
        var minDistance = double.MaxValue;
        var nearestIndex = 0;

        for (var index = 0; index < Palette.Count; index++)
        {
            Color color = Palette.ElementAt (index);
            double distance = DistanceAlgorithm.CalculateDistance (color, toTranslate);

            if (distance < minDistance)
            {
                minDistance = distance;
                nearestIndex = index;
            }
        }

        _nearestColorCache.TryAdd (toTranslate, nearestIndex);

        return nearestIndex;
    }
}
