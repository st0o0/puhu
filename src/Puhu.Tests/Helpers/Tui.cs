using Termina.Layout;
using Termina.Rendering;
using Termina.Terminal;

namespace Puhu.Tests.Helpers;

/// <summary>
/// Test helpers that render layout nodes through Termina's real rendering pipeline
/// (<see cref="RegionRenderContext"/> writing into a <see cref="VirtualTerminal"/>),
/// so assertions run against the same buffer the framework produces at runtime.
/// </summary>
internal static class Tui
{
    /// <summary>Measure + render <paramref name="layout"/> into a fresh virtual terminal.</summary>
    public static VirtualTerminal Render(ILayoutNode layout, int width, int height)
    {
        var terminal = new VirtualTerminal(width, height);
        var context = new RegionRenderContext(terminal, 0, 0, width, height);
        layout.Measure(new Size(width, height));
        layout.Render(context, new Rect(0, 0, width, height));
        return terminal;
    }

    /// <summary>
    /// The full row INCLUDING trailing spaces. <see cref="VirtualTerminal.GetLine"/> trims the
    /// right edge, which would drop border characters at the last column — tests need the raw row.
    /// </summary>
    public static string Row(this VirtualTerminal terminal, int y)
    {
        var chars = new char[terminal.Width];
        for (var x = 0; x < terminal.Width; x++)
        {
            chars[x] = terminal.GetChar(x, y);
        }
        return new string(chars);
    }

    /// <summary>The whole buffer as newline-joined full-width rows.</summary>
    public static string Snapshot(this VirtualTerminal terminal)
    {
        var rows = new string[terminal.Height];
        for (var y = 0; y < terminal.Height; y++)
        {
            rows[y] = terminal.Row(y);
        }
        return string.Join('\n', rows);
    }

    /// <summary>True when nothing was drawn (the buffer is all whitespace).</summary>
    public static bool IsBlank(this VirtualTerminal terminal) => string.IsNullOrWhiteSpace(terminal.Snapshot());
}
