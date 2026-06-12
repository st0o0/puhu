using Termina.Layout;
using Termina.Rendering;
using Termina.Terminal;

namespace Puhu.TUI.Nodes;

public sealed class TabBarNode : LayoutNode
{
    private static List<string> _allLabels = [];
    private static IReadOnlyList<string> _allRoutes = [];

    private readonly int _activeIndex;

    public static void RegisterPluginTabs(PluginRegistry registry)
    {
        var labels = new List<string>();
        var routes = new List<string>();
        foreach (var tab in registry.PluginTabs)
        {
            labels.Add(tab.Label);
            routes.Add(tab.Route);
        }

        _allLabels = labels;
        _allRoutes = routes;
    }

    public TabBarNode(int activeIndex)
    {
        _activeIndex = activeIndex;
        HeightConstraint = new SizeConstraint.Fixed(1);
        WidthConstraint = new SizeConstraint.Fill();
    }

    public static string GetRoute(int index) => _allRoutes[Math.Clamp(index, 0, _allRoutes.Count - 1)];

    public static int TabCount => _allRoutes.Count;

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
                ctx.SetForeground(Color.Black);
                ctx.SetBackground(Color.White);
            }
            else
            {
                ctx.SetForeground(Color.Gray);
            }

            ctx.WriteAt(x, 0, label);
            ctx.ResetColors();
            x += label.Length + 1;
        }
    }
}
