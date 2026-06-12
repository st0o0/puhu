using Puhu.Plugin;
using Termina.Layout;
using Termina.Rendering;

namespace Puhu.Nodes;

internal sealed class TabBarNode : LayoutNode
{
    private static List<string> _allLabels = [];
    private static IReadOnlyList<string> _allRoutes = [];

    private readonly int _activeIndex;
    private readonly ThemeDefinition _theme;

    public static void RegisterTabs(IReadOnlyList<PluginTabInfo> tabs)
    {
        _allLabels = tabs.Select(t => t.Label).ToList();
        _allRoutes = tabs.Select(t => t.Route).ToList();
    }

    public TabBarNode(int activeIndex, ThemeDefinition theme)
    {
        _activeIndex = activeIndex;
        _theme = theme;
        HeightConstraint = new SizeConstraint.Fixed(1);
        WidthConstraint = new SizeConstraint.Fill();
    }

    public static string GetRoute(int index) => _allRoutes[Math.Clamp(index, 0, _allRoutes.Count - 1)];

    public static int TabCount => _allRoutes.Count;

    public static int CurrentTabIndex { get; set; }

    public override Size Measure(Size available) => available with { Height = 1 };

    public override void Render(IRenderContext context, Rect bounds)
    {
        if (!bounds.HasArea)
        {
            return;
        }

        var ctx = context.CreateSubContext(bounds);
        ctx.Fill(0, 0, bounds.Width, 1);
        var x = 1;
        for (var i = 0; i < _allLabels.Count; i++)
        {
            var label = $" {_allLabels[i]} ";
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
