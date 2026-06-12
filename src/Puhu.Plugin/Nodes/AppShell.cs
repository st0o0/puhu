using Puhu.Plugin;
using Termina.Layout;
using Termina.Rendering;

namespace Puhu.Plugin.Nodes;

public static class AppShell
{
    public static ILayoutNode Wrap(ThemeDefinition theme, int activeTab, ILayoutNode content, params string[] keyHints)
    {
        return new PanelNode()
            .WithBorder(BorderStyle.Rounded)
            .WithBorderColor(theme.Border)
            .WithContent(Layouts.Vertical(
                new TabBarNode(activeTab, theme),
                new SeparatorNode(theme),
                Layouts.Vertical(content).Fill(),
                new SeparatorNode(theme),
                new KeyHintsNode(theme, keyHints)
            ));
    }
}
