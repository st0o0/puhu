using Puhu.Plugin;
using Termina.Layout;
using Termina.Rendering;
using Termina.Terminal;

namespace Puhu.Settings.Pages;

/// <summary>
/// Renders the full color table of a theme: one swatch + name + value per role,
/// then a sampled graph-gradient strip. Read-only view of a <see cref="ThemeDefinition"/>.
/// </summary>
internal sealed class ThemePaletteNode : LayoutNode
{
    private static readonly (string Name, Func<ThemeDefinition, Color> Get)[] Roles =
    [
        ("background", t => t.Background),
        ("foreground", t => t.Foreground),
        ("text-dim", t => t.TextDim),
        ("border", t => t.Border),
        ("panel-title", t => t.PanelTitle),
        ("accent", t => t.Accent),
        ("selection", t => t.Selection),
        ("sel-text", t => t.SelectionText),
        ("status-bar", t => t.StatusBar),
        ("status-text", t => t.StatusBarText),
        ("header", t => t.Header),
        ("warning", t => t.Warning),
        ("error", t => t.Error),
        ("success", t => t.Success),
    ];

    private readonly ThemeDefinition _theme;

    public ThemePaletteNode(ThemeDefinition theme)
    {
        _theme = theme;
        HeightConstraint = new SizeConstraint.Auto();
        WidthConstraint = new SizeConstraint.Fill();
    }

    public override Size Measure(Size available) =>
        new(available.Width, Math.Min(available.Height, Roles.Length + 1));

    public override void Render(IRenderContext context, Rect bounds)
    {
        if (!bounds.HasArea)
        {
            return;
        }

        var ctx = context.CreateSubContext(bounds);
        var y = 0;

        foreach (var (name, get) in Roles)
        {
            if (y >= bounds.Height)
            {
                break;
            }

            var color = get(_theme);
            ctx.SetForeground(color);
            ctx.WriteAt(0, y, "███");
            ctx.SetForeground(_theme.TextDim);
            ctx.WriteAt(4, y, $"{name,-12} {ColorFormat.Describe(color)}");
            y++;
        }

        if (y < bounds.Height)
        {
            var w = Math.Min(bounds.Width, 16);
            for (var x = 0; x < w; x++)
            {
                var t = w <= 1 ? 0f : x / (float)(w - 1);
                ctx.SetForeground(_theme.GraphGradient.Sample(t));
                ctx.WriteAt(x, y, "█");
            }

            ctx.SetForeground(_theme.TextDim);
            if (w + 1 < bounds.Width)
            {
                ctx.WriteAt(w + 1, y, "graph gradient");
            }
        }

        ctx.ResetColors();
    }
}
