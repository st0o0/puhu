using Puhu.Plugin;
using Termina.Input;

namespace Puhu.Tests;

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

    private sealed class FakeTabNavigator : ITabNavigator
    {
        private readonly string[] _routes;

        public int CurrentIndex { get; private set; }
        public bool HasTabs => _routes.Length > 0;

        public FakeTabNavigator(string[] routes, int currentIndex)
        {
            _routes = routes;
            CurrentIndex = currentIndex;
        }

        public void CycleTab(Action<string> navigate, int delta)
        {
            var count = _routes.Length;
            var next = (CurrentIndex + delta + count) % count;
            CurrentIndex = next;
            navigate(_routes[next]);
        }
    }
}
