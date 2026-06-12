using Puhu.Themes;
using Puhu.Plugin;
using Termina.Layout;
using Termina.Rendering;

namespace Puhu.Nodes;

public sealed class SeparatorNode : LayoutNode
{
    private readonly char _left;
    private readonly char _right;
    private readonly char _fill;

    public SeparatorNode(char left = '├', char right = '┤', char fill = '─')
    {
        _left = left;
        _right = right;
        _fill = fill;
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
        ctx.SetForeground(theme.Border);

        ctx.WriteAt(0, 0, _left);
        for (var x = 1; x < bounds.Width - 1; x++)
            ctx.WriteAt(x, 0, _fill);
        if (bounds.Width > 1)
            ctx.WriteAt(bounds.Width - 1, 0, _right);

        ctx.ResetColors();
    }
}
