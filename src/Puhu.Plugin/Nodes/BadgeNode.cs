using Termina.Layout;
using Termina.Rendering;
using Termina.Terminal;

namespace Puhu.Plugin.Nodes;

public sealed class BadgeNode : LayoutNode
{
    private readonly string _text;
    private readonly Color _foreground;
    private readonly Color _background;
    private readonly string? _icon;
    private readonly string _rendered;

    public BadgeNode(string text, Color foreground, Color background, string? icon = null)
    {
        _text = text;
        _foreground = foreground;
        _background = background;
        _icon = icon;
        _rendered = icon is not null ? $" {icon} {text} " : $" {text} ";
        HeightConstraint = new SizeConstraint.Fixed(1);
        WidthConstraint = new SizeConstraint.Auto();
    }

    public override Size Measure(Size available) =>
        new(Math.Min(_rendered.Length, available.Width), 1);

    public override void Render(IRenderContext context, Rect bounds)
    {
        if (!bounds.HasArea) return;
        var ctx = context.CreateSubContext(bounds);
        ctx.SetForeground(_foreground);
        ctx.SetBackground(_background);
        ctx.WriteAt(0, 0, _rendered.Length > bounds.Width ? _rendered[..bounds.Width] : _rendered);
        ctx.ResetColors();
    }
}
