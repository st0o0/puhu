using Termina.Layout;
using Termina.Rendering;
using Termina.Terminal;

namespace Puhu.Plugin.Nodes;

public sealed class KeyValueRowNode : LayoutNode
{
    private readonly string _label;
    private readonly string _value;
    private readonly int _labelWidth;
    private readonly Color _labelColor;
    private readonly Color _valueColor;
    private readonly bool _valueBold;

    public KeyValueRowNode(string label, string value, int labelWidth,
        Color labelColor, Color valueColor, bool valueBold = false)
    {
        _label = label;
        _value = value;
        _labelWidth = labelWidth;
        _labelColor = labelColor;
        _valueColor = valueColor;
        _valueBold = valueBold;

        HeightConstraint = new SizeConstraint.Fixed(1);
        WidthConstraint = new SizeConstraint.Fill();
    }

    public override Size Measure(Size available) => available with { Height = 1 };

    public override void Render(IRenderContext context, Rect bounds)
    {
        if (!bounds.HasArea) return;

        var ctx = context.CreateSubContext(bounds);

        ctx.SetForeground(_labelColor);
        ctx.WriteAt(0, 0, _label.PadRight(_labelWidth));

        ctx.SetForeground(_valueColor);
        if (_valueBold) ctx.SetDecoration(TextDecoration.Bold);
        ctx.WriteAt(_labelWidth, 0, _value);
        if (_valueBold) ctx.SetDecoration(TextDecoration.None);

        ctx.ResetColors();
    }
}
