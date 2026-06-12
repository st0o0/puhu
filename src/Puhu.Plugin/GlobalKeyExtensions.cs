using Termina.Input;

namespace Puhu.Plugin;

public static class GlobalKeyExtensions
{
    public static void RegisterGlobalKeys(
        this PageKeyBindings keyBindings,
        Action requestShutdown,
        Action<string> navigate,
        ITabNavigator tabNavigator)
    {
        keyBindings.Register(ConsoleKey.Escape, requestShutdown);

        if (tabNavigator.HasTabs)
        {
            keyBindings.Register(ConsoleKey.Tab, () => tabNavigator.CycleTab(navigate, 1));
            keyBindings.Register(ConsoleKey.Tab, ConsoleModifiers.Shift, () => tabNavigator.CycleTab(navigate, -1));
        }
    }
}