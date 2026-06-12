using Puhu.Plugin;
using Termina.Layout;
using Termina.Rendering;

namespace Puhu.Plugin.Nodes;

public sealed class KeyHintsNode : LayoutNode
{
    private readonly string[] _hints;
    private readonly ThemeDefinition _theme;

    public KeyHintsNode(ThemeDefinition theme, params string[] hints)
    {
        _hints = hints;
        _theme = theme;
        HeightConstraint = new SizeConstraint.Fixed(1);
        WidthConstraint = new SizeConstraint.Fill();
    }

    public override Size Measure(Size available) => available with { Height = 1 };

    public override void Render(IRenderContext context, Rect bounds)
    {
        if (!bounds.HasArea)
            return;

        var ctx = context.CreateSubContext(bounds);

        ctx.SetBackground(_theme.StatusBar);
        ctx.SetForeground(_theme.StatusBarText);
        ctx.Fill(0, 0, bounds.Width, 1);

        var x = 1;
        foreach (var hint in _hints)
        {
            if (x + hint.Length >= bounds.Width)
                break;

            var parts = hint.Split(':', 2);
            if (parts.Length == 2)
            {
                ctx.SetForeground(_theme.Accent);
                ctx.WriteAt(x, 0, parts[0]);
                x += parts[0].Length;

                ctx.SetForeground(_theme.StatusBarText);
                ctx.WriteAt(x, 0, ":" + parts[1]);
                x += parts[1].Length + 1;
            }
            else
            {
                ctx.WriteAt(x, 0, hint);
                x += hint.Length;
            }

            x += 2;
        }

        ctx.ResetColors();
    }
}
