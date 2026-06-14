using Puhu.Plugin;
using Termina.Layout;
using Termina.Rendering;
using Termina.Terminal;

namespace Puhu.Settings.Pages;

/// <summary>Visual chrome for the setup wizard: breadcrumb rail, status bar, and a horizontal rule.</summary>
internal static class WizardChrome
{
    private static readonly IReadOnlyList<(SetupStep Step, string Label)> StepList =
    [
        (SetupStep.Welcome, "welcome"),
        (SetupStep.Theme, "theme"),
        (SetupStep.Refresh, "refresh"),
        (SetupStep.TabOrder, "tabs"),
        (SetupStep.Plugins, "plugins"),
        (SetupStep.Done, "done"),
    ];

    public static int StepCount => StepList.Count;

    public static ILayoutNode Breadcrumb(SetupStep current, ThemeDefinition theme)
    {
        var parts = new List<ILayoutNode>();
        for (var i = 0; i < StepList.Count; i++)
        {
            if (i > 0)
            {
                parts.Add(new TextNode(" › ").WithForeground(theme.TextDim).NoWrap().WidthAuto());
            }

            var (step, label) = StepList[i];
            if (step == current)
            {
                parts.Add(new TextNode($"[{label}]").WithForeground(theme.Accent).Bold().NoWrap().WidthAuto());
            }
            else
            {
                var visited = (int)step < (int)current;
                parts.Add(new TextNode(label).WithForeground(visited ? theme.Foreground : theme.TextDim).NoWrap().WidthAuto());
            }
        }

        return Layouts.Horizontal(parts.ToArray());
    }

    public static ILayoutNode StatusBar(int index, int total, string hints, ThemeDefinition theme)
    {
        var dots = string.Join(" ", Enumerable.Range(0, total).Select(i => i <= index ? "●" : "○"));
        return Layouts.Horizontal(
            new TextNode(dots).WithForeground(theme.Accent).NoWrap().WidthAuto(),
            new TextNode($"   step {index + 1}/{total}   ").WithForeground(theme.TextDim).NoWrap().WidthAuto(),
            new TextNode(hints).WithForeground(theme.TextDim).NoWrap().WidthAuto());
    }

    public static string HintsFor(SetupStep step) => step switch
    {
        SetupStep.Welcome => "Enter start · S skip",
        SetupStep.TabOrder => "↑/↓ select · ⇧↑/↓ move · Enter · Esc · S",
        SetupStep.Plugins => "O toggle · Enter · Esc · S",
        SetupStep.Done => "Enter finish",
        _ => "↑/↓ choose · Enter · Esc · S",
    };
}

/// <summary>A full-width horizontal rule, one row tall.</summary>
internal sealed class RuleNode : LayoutNode
{
    private readonly Color _color;

    public RuleNode(Color color)
    {
        _color = color;
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
        ctx.SetForeground(_color);
        ctx.Fill(0, 0, bounds.Width, 1, '─');
        ctx.ResetColors();
    }
}
