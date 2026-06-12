using Puhu.Plugin;
using Puhu.Plugin.Nodes;
using Termina.Input;

namespace Puhu.Tests;

public sealed class GlobalKeyExtensionsTests
{
    [Fact]
    public void Escape_CallsShutdown()
    {
        var bindings = new PageKeyBindings();
        var shutdownCalled = false;

        bindings.RegisterGlobalKeys(() => shutdownCalled = true, _ => { });
        bindings.TryHandle(new ConsoleKeyInfo('\0', ConsoleKey.Escape, false, false, false));

        Assert.True(shutdownCalled);
    }

    [Fact]
    public void Tab_NavigatesToNextTab()
    {
        TabBarNode.RegisterTabs([
            new PluginTabInfo("A", "/a"),
            new PluginTabInfo("B", "/b")
        ]);
        TabBarNode.CurrentTabIndex = 0;

        var bindings = new PageKeyBindings();
        string? navigatedTo = null;

        bindings.RegisterGlobalKeys(() => { }, path => navigatedTo = path);
        bindings.TryHandle(new ConsoleKeyInfo('\0', ConsoleKey.Tab, false, false, false));

        Assert.Equal("/b", navigatedTo);
        Assert.Equal(1, TabBarNode.CurrentTabIndex);
    }

    [Fact]
    public void ShiftTab_NavigatesToPreviousTab()
    {
        TabBarNode.RegisterTabs([
            new PluginTabInfo("A", "/a"),
            new PluginTabInfo("B", "/b")
        ]);
        TabBarNode.CurrentTabIndex = 0;

        var bindings = new PageKeyBindings();
        string? navigatedTo = null;

        bindings.RegisterGlobalKeys(() => { }, path => navigatedTo = path);
        bindings.TryHandle(new ConsoleKeyInfo('\0', ConsoleKey.Tab, true, false, false));

        Assert.Equal("/b", navigatedTo);
    }

    [Fact]
    public void Tab_WrapsAround()
    {
        TabBarNode.RegisterTabs([
            new PluginTabInfo("A", "/a"),
            new PluginTabInfo("B", "/b")
        ]);
        TabBarNode.CurrentTabIndex = 1;

        var bindings = new PageKeyBindings();
        string? navigatedTo = null;

        bindings.RegisterGlobalKeys(() => { }, path => navigatedTo = path);
        bindings.TryHandle(new ConsoleKeyInfo('\0', ConsoleKey.Tab, false, false, false));

        Assert.Equal("/a", navigatedTo);
        Assert.Equal(0, TabBarNode.CurrentTabIndex);
    }
}
