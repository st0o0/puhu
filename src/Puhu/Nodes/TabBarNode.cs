using Puhu.Plugin;
using Termina.Layout;
using Termina.Rendering;

namespace Puhu.Nodes;

internal sealed class TabBarNode : LayoutNode
{
    private readonly int _activeIndex;
    private readonly ThemeDefinition _theme;

    public TabBarNode(int activeIndex, ThemeDefinition theme)
    {
        _activeIndex = activeIndex;
        _theme = theme;
        HeightConstraint = new SizeConstraint.Fixed(1);
        WidthConstraint = new SizeConstraint.Fill();
    }

    public override Size Measure(Size available) => available with { Height = 1 };

    public override void Render(IRenderContext context, Rect bounds)
    {
        if (!bounds.HasArea)
        {
            return;
        }

        var ctx = context.CreateSubContext(bounds);
        ctx.Fill(0, 0, bounds.Width, 1);
        var labels = TabRegistry.Labels;
        var x = 1;
        for (var i = 0; i < labels.Count; i++)
        {
            var label = $" {labels[i]} ";
            if (i == _activeIndex)
            {
                ctx.SetForeground(_theme.SelectionText);
                ctx.SetBackground(_theme.Selection);
            }
            else
            {
                ctx.SetForeground(_theme.TextDim);
            }

            ctx.WriteAt(x, 0, label);
            ctx.ResetColors();
            x += label.Length + 1;
        }
    }
}
