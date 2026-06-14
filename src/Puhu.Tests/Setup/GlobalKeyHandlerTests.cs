using Puhu.Plugin;
using Puhu.Tests.Fakes;
using Termina.Input;

namespace Puhu.Tests.Setup;

public sealed class GlobalKeyExtensionsTests
{
    [Fact]
    public void Escape_CallsShutdown()
    {
        var bindings = new PageKeyBindings();
        var shutdownCalled = false;
        var tabNav = new FakeTabNavigator([], 0);

        bindings.RegisterGlobalKeys(() => shutdownCalled = true, _ => { }, tabNav);
        bindings.TryHandle(new ConsoleKeyInfo('\0', ConsoleKey.Escape, false, false, false));

        Assert.True(shutdownCalled);
    }

    [Fact]
    public void Tab_NavigatesToNextTab()
    {
        var tabNav = new FakeTabNavigator(["/a", "/b"], 0);
        var bindings = new PageKeyBindings();
        string? navigatedTo = null;

        bindings.RegisterGlobalKeys(() => { }, path => navigatedTo = path, tabNav);
        bindings.TryHandle(new ConsoleKeyInfo('\0', ConsoleKey.Tab, false, false, false));

        Assert.Equal("/b", navigatedTo);
        Assert.Equal(1, tabNav.CurrentIndex);
    }

    [Fact]
    public void ShiftTab_NavigatesToPreviousTab()
    {
        var tabNav = new FakeTabNavigator(["/a", "/b"], 0);
        var bindings = new PageKeyBindings();
        string? navigatedTo = null;

        bindings.RegisterGlobalKeys(() => { }, path => navigatedTo = path, tabNav);
        bindings.TryHandle(new ConsoleKeyInfo('\0', ConsoleKey.Tab, true, false, false));

        Assert.Equal("/b", navigatedTo);
    }

    [Fact]
    public void Tab_WrapsAround()
    {
        var tabNav = new FakeTabNavigator(["/a", "/b"], 1);
        var bindings = new PageKeyBindings();
        string? navigatedTo = null;

        bindings.RegisterGlobalKeys(() => { }, path => navigatedTo = path, tabNav);
        bindings.TryHandle(new ConsoleKeyInfo('\0', ConsoleKey.Tab, false, false, false));

        Assert.Equal("/a", navigatedTo);
        Assert.Equal(0, tabNav.CurrentIndex);
    }
}
