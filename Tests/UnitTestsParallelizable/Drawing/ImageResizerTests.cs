using Xunit;
using Terminal.Gui.Drawing;

namespace Terminal.Gui.DrawingTests;

public class ImageResizerTests
{
    [Fact]
    public void Resize_SameSize_ReturnsOriginal()
    {
        var source = new Color[2, 2] {
            { new Color(255, 0, 0), new Color(0, 255, 0) },
            { new Color(0, 0, 255), new Color(255, 255, 255) }
        };

        var result = ImageResizer.Resize(source, 2, 2);

        Assert.Same(source, result);
    }

    [Fact]
    public void Resize_Larger_Interpolates()
    {
        var source = new Color[1, 1] { { new Color(255, 0, 0) } };
        var result = ImageResizer.Resize(source, 2, 2);

        Assert.Equal(2, result.GetLength(0));
        Assert.Equal(2, result.GetLength(1));
        Assert.Equal(source[0, 0], result[0, 0]);
        Assert.Equal(source[0, 0], result[1, 1]);
    }

    [Fact]
    public void Resize_Smaller_Interpolates()
    {
        var source = new Color[2, 2] {
            { new Color(255, 0, 0), new Color(0, 255, 0) },
            { new Color(0, 0, 255), new Color(255, 255, 255) }
        };
        var result = ImageResizer.Resize(source, 1, 1);

        Assert.Equal(1, result.GetLength(0));
        Assert.Equal(1, result.GetLength(1));
        Assert.Equal(source[0, 0], result[0, 0]);
    }

    [Fact]
    public void Resize_ZeroSize_ReturnsEmpty()
    {
        var source = new Color[2, 2];
        var result = ImageResizer.Resize(source, 0, 2);
        Assert.Equal(0, result.GetLength(0));
    }
}
