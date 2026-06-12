using Puhu.Plugin;

namespace Puhu.Nodes;

/// <summary>
/// Central registry of plugin tabs (labels, routes, active index).
/// Replaces the former TabBarNode statics.
/// </summary>
internal static class TabRegistry
{
    private static List<string> _labels = [];
    private static IReadOnlyList<string> _routes = [];

    public static IReadOnlyList<string> Labels => _labels;

    public static int TabCount => _routes.Count;

    public static int CurrentTabIndex { get; set; }

    public static void RegisterTabs(IReadOnlyList<PluginTabInfo> tabs)
    {
        _labels = tabs.Select(t => t.Label).ToList();
        _routes = tabs.Select(t => t.Route).ToList();
        CurrentTabIndex = 0;
    }

    public static string GetRoute(int index) => _routes[Math.Clamp(index, 0, _routes.Count - 1)];
}
