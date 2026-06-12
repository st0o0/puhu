using Termina.Layout;

namespace Puhu.Plugin.Nodes;

public static class AppShell
{
    public static ILayoutNode Wrap(ThemeDefinition theme, int activeTab, ILayoutNode content, params string[] keyHints)
        => new AppShellNode(theme, activeTab, content, keyHints);
}