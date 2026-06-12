using Termina.Layout;
using Termina.Rendering;

namespace Puhu.Plugin.Nodes;

public sealed class AppShellNode : LayoutNode
{
    private readonly ThemeDefinition _theme;
    private readonly TabBarNode _tabBar;
    private readonly KeyHintsNode _keyHints;
    private readonly ILayoutNode _content;

    public AppShellNode(ThemeDefinition theme, int activeTab, ILayoutNode content, params string[] keyHints)
    {
        _theme = theme;
        _tabBar = new TabBarNode(activeTab, theme);
        _keyHints = new KeyHintsNode(theme, keyHints);
        _content = content;
        HeightConstraint = new SizeConstraint.Fill();
        WidthConstraint = new SizeConstraint.Fill();
    }

    public override Size Measure(Size available) => available;

    // Layout rows:
    //   0      ╭────────╮  top border
    //   1      │ tabs   │  tab bar
    //   2      ├────────┤  separator
    //   3..n-4 │content │  content area
    //   n-3    ├────────┤  separator
    //   n-2    │ hints  │  key hints
    //   n-1    ╰────────╯  bottom border

    public override void Render(IRenderContext context, Rect bounds)
    {
        if (!bounds.HasArea || bounds.Width < 4 || bounds.Height < 6)
        {
            return;
        }

        var ctx = context.CreateSubContext(bounds);
        var w = bounds.Width;
        var h = bounds.Height;

        ctx.SetForeground(_theme.Border);

        DrawHorizontal(ctx, 0, w, '╭', '─', '╮');
        DrawVerticalBorders(ctx, 1, w);
        DrawHorizontal(ctx, 2, w, '├', '─', '┤');

        for (var y = 3; y <= h - 4; y++)
        {
            DrawVerticalBorders(ctx, y, w);
        }

        DrawHorizontal(ctx, h - 3, w, '├', '─', '┤');
        DrawVerticalBorders(ctx, h - 2, w);
        DrawHorizontal(ctx, h - 1, w, '╰', '─', '╯');

        ctx.ResetColors();

        var inner = w - 2;
        _tabBar.Render(context, new Rect(bounds.X + 1, bounds.Y + 1, inner, 1));
        _content.Render(context, new Rect(bounds.X + 1, bounds.Y + 3, inner, Math.Max(1, h - 6)));
        _keyHints.Render(context, new Rect(bounds.X + 1, bounds.Y + h - 2, inner, 1));
    }

    private static void DrawHorizontal(IRenderContext ctx, int y, int w, char left, char fill, char right)
    {
        var line = string.Create(w, (left, fill, right), static (span, state) =>
        {
            span[0] = state.left;
            span[^1] = state.right;
            span[1..^1].Fill(state.fill);
        });
        ctx.WriteAt(0, y, line);
    }

    private static void DrawVerticalBorders(IRenderContext ctx, int y, int w)
    {
        ctx.WriteAt(0, y, "│");
        ctx.WriteAt(w - 1, y, "│");
    }

    public override void Dispose()
    {
        _tabBar.Dispose();
        _keyHints.Dispose();
        if (_content is IDisposable d)
        {
            d.Dispose();
        }

        base.Dispose();
    }
}
