using Puhu.Plugin;

namespace Puhu.Nodes;

/// <summary>
/// Reorders the plugin tab list according to a saved order of routes. Known
/// routes appear in saved order; tabs not present in the saved order are
/// appended in their original order; saved routes that no longer resolve to a
/// tab are dropped.
/// </summary>
internal static class TabOrder
{
    public static IReadOnlyList<PluginTabInfo> Apply(
        IReadOnlyList<string> savedOrder,
        IReadOnlyList<PluginTabInfo> tabs)
    {
        var byRoute = new Dictionary<string, PluginTabInfo>();
        foreach (var tab in tabs)
        {
            byRoute[tab.Route] = tab;
        }

        var result = new List<PluginTabInfo>(tabs.Count);
        var used = new HashSet<string>();

        foreach (var route in savedOrder)
        {
            if (used.Add(route) && byRoute.TryGetValue(route, out var tab))
            {
                result.Add(tab);
            }
        }

        foreach (var tab in tabs)
        {
            if (!used.Contains(tab.Route))
            {
                result.Add(tab);
            }
        }

        return result;
    }
}
