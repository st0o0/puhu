using Termina.Layout;
using Termina.Rendering;
using Termina.Terminal;

namespace Puhu.Plugin.Tests;

/// <summary>
/// Renders layout nodes through Termina's real pipeline (<see cref="RegionRenderContext"/>
/// into a <see cref="VirtualTerminal"/>) so tests assert against the framework's own buffer.
/// </summary>
internal static class Tui
{
    public static VirtualTerminal Render(ILayoutNode layout, int width, int height)
    {
        var terminal = new VirtualTerminal(width, height);
        var context = new RegionRenderContext(terminal, 0, 0, width, height);
        layout.Measure(new Size(width, height));
        layout.Render(context, new Rect(0, 0, width, height));
        return terminal;
    }

    /// <summary>The full row INCLUDING trailing spaces (<see cref="VirtualTerminal.GetLine"/> trims them).</summary>
    public static string Row(this VirtualTerminal terminal, int y)
    {
        var chars = new char[terminal.Width];
        for (var x = 0; x < terminal.Width; x++)
        {
            chars[x] = terminal.GetChar(x, y);
        }
        return new string(chars);
    }
}
