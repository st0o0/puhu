using Puhu.Themes;
using Puhu.Plugin;
using Termina.Layout;
using Termina.Rendering;
using Termina.Terminal;

namespace Puhu.Nodes;

public static class AppShell
{
    public static ILayoutNode Wrap(int activeTab, ILayoutNode content, params string[] keyHints)
    {
        var theme = ThemeService.Instance.Current;

        return new PanelNode()
            .WithBorder(BorderStyle.Rounded)
            .WithBorderColor(theme.Border)
            .WithContent(Layouts.Vertical(
                new TabBarNode(activeTab),
                new SeparatorNode(),
                Layouts.Vertical(content).Fill(),
                new SeparatorNode(),
                new KeyHintsNode(keyHints)
            ));
    }
}
