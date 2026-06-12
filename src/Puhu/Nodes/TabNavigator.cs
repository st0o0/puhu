using Puhu.Plugin;

namespace Puhu.Nodes;

internal sealed class TabNavigator : ITabNavigator
{
    public bool HasTabs => TabRegistry.TabCount > 0;

    public void CycleTab(Action<string> navigate, int delta)
    {
        var count = TabRegistry.TabCount;
        var next = (TabRegistry.CurrentTabIndex + delta + count) % count;
        TabRegistry.CurrentTabIndex = next;
        navigate(TabRegistry.GetRoute(next));
    }
}
