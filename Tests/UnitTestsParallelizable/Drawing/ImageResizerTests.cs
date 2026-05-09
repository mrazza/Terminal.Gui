namespace DrawingTests;

public class ImageResizerTests
{
    [Fact]
    public void Resize_NullSource_ReturnsEmpty()
    {
        var result = Terminal.Gui.Drawing.ImageResizer.Resize(null, 10, 10);
        Assert.Equal(0, result.GetLength(0));
        Assert.Equal(0, result.GetLength(1));
    }

    [Fact]
    public void Resize_ZeroDimensions_ReturnsEmpty()
    {
        var source = new Terminal.Gui.Drawing.Color[1, 1];
        var result = Terminal.Gui.Drawing.ImageResizer.Resize(source, 0, 10);
        Assert.Equal(0, result.GetLength(0));
        
        result = Terminal.Gui.Drawing.ImageResizer.Resize(source, 10, 0);
        Assert.Equal(0, result.GetLength(1));
    }

    [Fact]
    public void Resize_SameDimensions_ReturnsSource()
    {
        var source = new Terminal.Gui.Drawing.Color[2, 2];
        var result = Terminal.Gui.Drawing.ImageResizer.Resize(source, 2, 2);
        Assert.Same(source, result);
    }

    [Fact]
    public void Resize_SimpleScale()
    {
        var source = new Terminal.Gui.Drawing.Color[2, 2];
        source[0, 0] = new Terminal.Gui.Drawing.Color(255, 0, 0);
        source[1, 1] = new Terminal.Gui.Drawing.Color(0, 255, 0);

        var result = Terminal.Gui.Drawing.ImageResizer.Resize(source, 4, 4);
        Assert.Equal(4, result.GetLength(0));
        Assert.Equal(4, result.GetLength(1));
        Assert.Equal(source[0, 0], result[0, 0]);
        Assert.Equal(source[0, 0], result[1, 1]);
        Assert.Equal(source[1, 1], result[2, 2]);
        Assert.Equal(source[1, 1], result[3, 3]);
    }
}