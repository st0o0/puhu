using Termina.Layout;
using Termina.Rendering;
using Termina.Terminal;

namespace Puhu.Plugin.Nodes;

public sealed class SubNavSeparatorNode : LayoutNode
{
    private readonly Color _color;

    public SubNavSeparatorNode(Color color)
    {
        _color = color;
        HeightConstraint = new SizeConstraint.Fixed(1);
        WidthConstraint = new SizeConstraint.Fill();
    }

    public override Size Measure(Size available) => available with { Height = 1 };

    public override void Render(IRenderContext context, Rect bounds)
    {
        if (!bounds.HasArea) return;
        var ctx = context.CreateSubContext(bounds);
        ctx.SetForeground(_color);
        ctx.Fill(0, 0, bounds.Width, 1, '━');
        ctx.ResetColors();
    }
}
