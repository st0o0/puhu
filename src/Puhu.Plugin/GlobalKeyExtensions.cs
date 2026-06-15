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

    public static void RegisterGlobalKeys(
        this PageKeyBindings keyBindings,
        Action requestShutdown,
        Action<string> navigate,
        ITabNavigator tabNavigator,
        IRefreshController refreshController)
    {
        keyBindings.RegisterGlobalKeys(requestShutdown, navigate, tabNavigator);

        keyBindings.Register(ConsoleKey.F, ConsoleModifiers.Control, refreshController.SpeedUp);
        keyBindings.Register(ConsoleKey.S, ConsoleModifiers.Control, refreshController.SlowDown);
        keyBindings.Register(ConsoleKey.P, refreshController.TogglePause);
    }
}