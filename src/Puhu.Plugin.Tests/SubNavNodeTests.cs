using Puhu.Plugin;
using Puhu.Plugin.Nodes;
using R3;
using Termina.Input;

namespace Puhu.Plugin.Tests;

public enum TestView { Alpha, Beta, Gamma }

public sealed class SubNavNodeTests
{
    [Fact]
    public void Ctor_StoresItems()
    {
        var view = new ReactiveProperty<TestView>(TestView.Alpha);
        var keys = new PageKeyBindings();
        var theme = new ThemeDefinition();

        var node = new SubNavNode<TestView>(view, keys, theme,
            (ConsoleKey.A, "Alpha", TestView.Alpha),
            (ConsoleKey.B, "Beta", TestView.Beta));

        Assert.Equal(2, node.ItemCount);
    }

    [Fact]
    public void Ctor_RegistersKeysOnKeyBindings()
    {
        var view = new ReactiveProperty<TestView>(TestView.Alpha);
        var keys = new PageKeyBindings();
        var theme = new ThemeDefinition();

        _ = new SubNavNode<TestView>(view, keys, theme,
            (ConsoleKey.A, "Alpha", TestView.Alpha),
            (ConsoleKey.B, "Beta", TestView.Beta),
            (ConsoleKey.G, "Gamma", TestView.Gamma));

        Assert.Equal(3, keys.Count);
    }

    [Fact]
    public void KeyPress_SetsActiveView()
    {
        var view = new ReactiveProperty<TestView>(TestView.Alpha);
        var keys = new PageKeyBindings();
        var theme = new ThemeDefinition();

        _ = new SubNavNode<TestView>(view, keys, theme,
            (ConsoleKey.A, "Alpha", TestView.Alpha),
            (ConsoleKey.B, "Beta", TestView.Beta));

        keys.TryHandle(new ConsoleKeyInfo('b', ConsoleKey.B, false, false, false));

        Assert.Equal(TestView.Beta, view.Value);
    }

    [Fact]
    public void KeyPress_SwitchesBackToFirst()
    {
        var view = new ReactiveProperty<TestView>(TestView.Beta);
        var keys = new PageKeyBindings();
        var theme = new ThemeDefinition();

        _ = new SubNavNode<TestView>(view, keys, theme,
            (ConsoleKey.A, "Alpha", TestView.Alpha),
            (ConsoleKey.B, "Beta", TestView.Beta));

        keys.TryHandle(new ConsoleKeyInfo('a', ConsoleKey.A, false, false, false));

        Assert.Equal(TestView.Alpha, view.Value);
    }

    [Fact]
    public void Ctor_WithIThemeService_Works()
    {
        var view = new ReactiveProperty<TestView>(TestView.Alpha);
        var keys = new PageKeyBindings();
        var themeService = new FakeThemeService();

        var node = new SubNavNode<TestView>(view, keys, themeService,
            (ConsoleKey.A, "Alpha", TestView.Alpha));

        Assert.Equal(1, node.ItemCount);
    }

    private sealed class FakeThemeService : IThemeService
    {
        public ThemeDefinition Current { get; } = new();
    }
}
