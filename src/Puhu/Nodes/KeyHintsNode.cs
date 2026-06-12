using Puhu.Themes;
using Puhu.Plugin;
using Termina.Layout;
using Termina.Rendering;

namespace Puhu.Nodes;

public sealed class KeyHintsNode : LayoutNode
{
    private readonly string[] _hints;

    public KeyHintsNode(params string[] hints)
    {
        _hints = hints;
        HeightConstraint = new SizeConstraint.Fixed(1);
        WidthConstraint = new SizeConstraint.Fill();
    }

    public override Size Measure(Size available) => available with { Height = 1 };

    public override void Render(IRenderContext context, Rect bounds)
    {
        if (!bounds.HasArea)
            return;

        var ctx = context.CreateSubContext(bounds);
        var theme = ThemeService.Instance.Current;

        ctx.SetBackground(theme.StatusBar);
        ctx.SetForeground(theme.StatusBarText);
        ctx.Fill(0, 0, bounds.Width, 1);

        var x = 1;
        foreach (var hint in _hints)
        {
            if (x + hint.Length >= bounds.Width)
                break;

            var parts = hint.Split(':', 2);
            if (parts.Length == 2)
            {
                ctx.SetForeground(theme.Accent);
                ctx.WriteAt(x, 0, parts[0]);
                x += parts[0].Length;

                ctx.SetForeground(theme.StatusBarText);
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
