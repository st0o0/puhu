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

    /// <summary>
    /// Replace the tab list while keeping the user on the same tab (matched by
    /// route). Unlike <see cref="RegisterTabs"/>, this does not reset the active
    /// index to 0. If the active route is gone, the index is clamped into range.
    /// </summary>
    public static void Reorder(IReadOnlyList<PluginTabInfo> tabs)
    {
        var activeRoute = CurrentTabIndex >= 0 && CurrentTabIndex < _routes.Count
            ? _routes[CurrentTabIndex]
            : null;

        _labels = tabs.Select(t => t.Label).ToList();
        _routes = tabs.Select(t => t.Route).ToList();

        if (activeRoute is not null)
        {
            var newIndex = -1;
            for (var i = 0; i < _routes.Count; i++)
            {
                if (_routes[i] == activeRoute)
                {
                    newIndex = i;
                    break;
                }
            }

            CurrentTabIndex = newIndex >= 0
                ? newIndex
                : Math.Clamp(CurrentTabIndex, 0, Math.Max(0, _routes.Count - 1));
        }
        else
        {
            CurrentTabIndex = Math.Clamp(CurrentTabIndex, 0, Math.Max(0, _routes.Count - 1));
        }
    }
}
