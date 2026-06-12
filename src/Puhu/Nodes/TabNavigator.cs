using Puhu.Plugin;

namespace Puhu.Nodes;

internal sealed class TabNavigator : ITabNavigator
{
    public bool HasTabs => TabBarNode.TabCount > 0;

    public void CycleTab(Action<string> navigate, int delta)
    {
        var count = TabBarNode.TabCount;
        var next = (TabBarNode.CurrentTabIndex + delta + count) % count;
        TabBarNode.CurrentTabIndex = next;
        navigate(TabBarNode.GetRoute(next));
    }
}
