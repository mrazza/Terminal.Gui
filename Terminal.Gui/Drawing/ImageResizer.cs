namespace Terminal.Gui.Drawing;

/// <summary>
/// Provides methods for resizing image data.
/// </summary>
public static class ImageResizer
{
    /// <summary>
    /// Resizes a 2D array of colors using nearest-neighbor interpolation.
    /// </summary>
    /// <param name="source">The source color array.</param>
    /// <param name="width">The target width.</param>
    /// <param name="height">The target height.</param>
    /// <returns>A new color array with the specified dimensions.</returns>
    public static Color[,] Resize(Color[,]? source, int width, int height)
    {
        if (source == null || width <= 0 || height <= 0)
        {
            return new Color[0, 0];
        }

        int sourceWidth = source.GetLength(0);
        int sourceHeight = source.GetLength(1);

        if (sourceWidth == width && sourceHeight == height)
        {
            return source;
        }

        var result = new Color[width, height];

        float scaleX = (float)sourceWidth / width;
        float scaleY = (float)sourceHeight / height;

        for (int y = 0; y < height; y++)
        {
            int sourceY = (int)(y * scaleY);
            for (int x = 0; x < width; x++)
            {
                int sourceX = (int)(x * scaleX);
                result[x, y] = source[sourceX, sourceY];
            }
        }

        return result;
    }
}