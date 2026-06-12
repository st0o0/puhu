using Termina.Layout;
using Termina.Rendering;
using Termina.Terminal;

namespace Puhu.Tests;

/// <summary>
/// Char-grid implementation of IRenderContext for render assertions.
/// Row(y) returns the line as string; Writes records colors.
/// </summary>
internal sealed class RenderTestContext : IRenderContext
{
    private readonly char[] _grid;
    private readonly RenderTestContext _root;
    private readonly int _offsetX;
    private readonly int _offsetY;
    private Color? _fg;
    private Color? _bg;

    public List<(int X, int Y, string Text, Color? Fg, Color? Bg)> Writes { get; }

    public RenderTestContext(int width, int height)
    {
        Width = width;
        Height = height;
        _grid = new char[width * height];
        Array.Fill(_grid, ' ');
        _root = this;
        Writes = [];
    }

    private RenderTestContext(RenderTestContext root, int offsetX, int offsetY, int width, int height)
    {
        _root = root;
        _offsetX = offsetX;
        _offsetY = offsetY;
        Width = width;
        Height = height;
        _grid = root._grid;
        Writes = root.Writes;
    }

    public int Width { get; }
    public int Height { get; }

    public string Row(int y)
    {
        var rootRow = _offsetY + y;
        return new string(_root._grid, rootRow * _root.Width + _offsetX, Width);
    }

    public void WriteAt(int x, int y, string text)
    {
        Writes.Add((_offsetX + x, _offsetY + y, text, _fg, _bg));
        for (var i = 0; i < text.Length; i++)
        {
            Put(x + i, y, text[i]);
        }
    }

    public void WriteAt(int x, int y, char c)
    {
        Writes.Add((_offsetX + x, _offsetY + y, c.ToString(), _fg, _bg));
        Put(x, y, c);
    }

    public void SetForeground(Color color) => _fg = color;
    public void SetBackground(Color color) => _bg = color;

    public void ResetColors()
    {
        _fg = null;
        _bg = null;
    }

    public void SetDecoration(TextDecoration decoration) { }
    public void ApplyStyle(TextStyle style) { }

    public void Fill(int x, int y, int width, int height, char c = ' ')
    {
        for (var dy = 0; dy < height; dy++)
        {
            for (var dx = 0; dx < width; dx++)
            {
                Put(x + dx, y + dy, c);
            }
        }
    }

    public void Clear() => Fill(0, 0, Width, Height);

    public IRenderContext CreateSubContext(Rect bounds)
    {
        var x = Math.Max(0, bounds.X);
        var y = Math.Max(0, bounds.Y);
        var right = Math.Min(Width, bounds.X + bounds.Width);
        var bottom = Math.Min(Height, bounds.Y + bounds.Height);
        var width = Math.Max(0, right - x);
        var height = Math.Max(0, bottom - y);
        return new RenderTestContext(_root, _offsetX + x, _offsetY + y, width, height);
    }

    private void Put(int x, int y, char c)
    {
        if (x < 0 || y < 0 || x >= Width || y >= Height)
        {
            return;
        }

        var rootX = _offsetX + x;
        var rootY = _offsetY + y;
        _root._grid[rootY * _root.Width + rootX] = c;
    }
}
