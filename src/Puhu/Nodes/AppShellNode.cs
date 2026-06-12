using Puhu.Plugin;
using Termina.Layout;
using Termina.Rendering;

namespace Puhu.Nodes;

/// <summary>
/// Full-screen btop-style shell panel: TopBarNode as top border,
/// key hints embedded in the bottom border, content in between.
/// </summary>
internal sealed class AppShellNode : LayoutNode
{
    private readonly IThemeService _themeService;
    private readonly TopBarNode _topBar;
    private readonly ILayoutNode _content;
    private readonly string[] _keyHints;

    public AppShellNode(IThemeService themeService, ILayoutNode content, params string[] keyHints)
    {
        _themeService = themeService;
        _topBar = new TopBarNode(themeService);
        _content = content;
        _keyHints = keyHints;
        HeightConstraint = new SizeConstraint.Fill();
        WidthConstraint = new SizeConstraint.Fill();
    }

    public override Size Measure(Size available) => available;

    public override void Render(IRenderContext context, Rect bounds)
    {
        if (!bounds.HasArea || bounds.Width < 12 || bounds.Height < 3)
        {
            return;
        }

        var theme = _themeService.Current;
        var w = bounds.Width;
        var h = bounds.Height;
        var ctx = context.CreateSubContext(bounds);

        _topBar.Render(context, new Rect(bounds.X, bounds.Y, w, 1));

        ctx.SetForeground(theme.Border);
        for (var y = 1; y < h - 1; y++)
        {
            ctx.WriteAt(0, y, '│');
            ctx.WriteAt(w - 1, y, '│');
        }

        RenderBottomBorder(ctx, theme, w, h - 1);
        ctx.ResetColors();

        _content.Render(context, new Rect(bounds.X + 1, bounds.Y + 1, w - 2, h - 2));
    }

    private void RenderBottomBorder(IRenderContext ctx, ThemeDefinition theme, int w, int y)
    {
        ctx.SetForeground(theme.Border);
        ctx.WriteAt(0, y, '╰');
        for (var x = 1; x < w - 1; x++)
        {
            ctx.WriteAt(x, y, '─');
        }

        ctx.WriteAt(w - 1, y, '╯');

        var cx = 1;
        foreach (var hint in _keyHints)
        {
            var parts = hint.Split(':', 2);
            var key = parts[0];
            var label = parts.Length == 2 ? parts[1].ToLowerInvariant() : null;
            var innerLength = label is null ? key.Length + 2 : key.Length + label.Length + 3;
            var total = innerLength + 2;

            if (cx + total >= w - 1)
            {
                break;
            }

            ctx.SetForeground(theme.Border);
            ctx.WriteAt(cx, y, '┤');
            ctx.SetForeground(theme.Accent);
            ctx.WriteAt(cx + 1, y, $" {key}");

            if (label is not null)
            {
                ctx.SetForeground(theme.TextDim);
                ctx.WriteAt(cx + 2 + key.Length, y, $" {label} ");
            }
            else
            {
                ctx.SetForeground(theme.TextDim);
                ctx.WriteAt(cx + 2 + key.Length, y, ' ');
            }

            ctx.SetForeground(theme.Border);
            ctx.WriteAt(cx + total - 1, y, '├');
            cx += total + 1;
        }
    }

    public override void OnActivate()
    {
        if (_content is IActivatableNode contentNode)
        {
            contentNode.OnActivate();
        }

        base.OnActivate();
    }

    public override void OnDeactivate()
    {
        if (_content is IActivatableNode contentNode)
        {
            contentNode.OnDeactivate();
        }

        base.OnDeactivate();
    }

    public override void Dispose()
    {
        _topBar.Dispose();
        if (_content is IDisposable d)
        {
            d.Dispose();
        }

        base.Dispose();
    }
}
