using Puhu.Plugin.Nodes;
using Termina.Input;

namespace Puhu.Plugin;

public static class GlobalKeyExtensions
{
    public static void RegisterGlobalKeys(
        this PageKeyBindings keyBindings,
        Action requestShutdown,
        Action<string> navigate)
    {
        keyBindings.Register(ConsoleKey.Escape, requestShutdown);

        if (TabBarNode.TabCount > 0)
        {
            keyBindings.Register(ConsoleKey.Tab, () =>
                CycleTab(navigate, 1));
            keyBindings.Register(ConsoleKey.Tab, ConsoleModifiers.Shift, () =>
                CycleTab(navigate, -1));
        }
    }

    private static void CycleTab(Action<string> navigate, int delta)
    {
        var count = TabBarNode.TabCount;
        var next = (TabBarNode.CurrentTabIndex + delta + count) % count;
        TabBarNode.CurrentTabIndex = next;
        navigate(TabBarNode.GetRoute(next));
    }
}
