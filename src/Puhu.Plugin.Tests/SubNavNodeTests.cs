using Puhu.Plugin;
using Puhu.Plugin.Nodes;
using R3;
using Termina.Input;
using Termina.Layout;
using Termina.Rendering;
using Termina.Terminal;

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

    [Fact]
    public void Render_ActiveItem_HasSelectionBlockWithKeyAndLowercaseLabel()
    {
        var view = new ReactiveProperty<TestView>(TestView.Alpha);
        var node = new SubNavNode<TestView>(view, new PageKeyBindings(), new ThemeDefinition(),
            (ConsoleKey.D1, "Alpha", TestView.Alpha),
            (ConsoleKey.D2, "Beta", TestView.Beta));

        var term = Tui.Render(node, 40, 1);

        Assert.Contains(" 1 alpha ", term.Row(0));
        Assert.Contains("2 beta", term.Row(0));
        Assert.Equal("  1 alpha   2 beta", term.Row(0).TrimEnd());
    }

    [Fact]
    public void Render_ActiveItem_UsesSelectionColors()
    {
        var theme = new ThemeDefinition();
        var view = new ReactiveProperty<TestView>(TestView.Alpha);
        var node = new SubNavNode<TestView>(view, new PageKeyBindings(), theme,
            (ConsoleKey.D1, "Alpha", TestView.Alpha));

        var term = Tui.Render(node, 40, 1);

        // The active item " 1 alpha " is drawn with selection colors; sample a cell inside it.
        var x = term.Row(0).IndexOf("1 alpha", StringComparison.Ordinal);
        Assert.Equal(theme.Selection, term.GetBackground(x, 0));
        Assert.Equal(theme.SelectionText, term.GetForeground(x, 0));
    }

    [Fact]
    public void Render_UsesLiveThemeFromService()
    {
        var view = new ReactiveProperty<TestView>(TestView.Alpha);
        var themeService = new MutableFakeThemeService();
        var node = new SubNavNode<TestView>(view, new PageKeyBindings(), themeService,
            (ConsoleKey.D1, "Alpha", TestView.Alpha));

        themeService.Current = new ThemeDefinition { Selection = Color.FromHex("#123456") };
        var term = Tui.Render(node, 40, 1);

        var x = term.Row(0).IndexOf("1 alpha", StringComparison.Ordinal);
        Assert.Equal(Color.FromHex("#123456"), term.GetBackground(x, 0));
    }

    private sealed class FakeThemeService : IThemeService
    {
        public ThemeDefinition Current { get; } = new();
        public string? CurrentThemeName => null;
        public Observable<ThemeDefinition> Changes => Observable.Empty<ThemeDefinition>();
        public IReadOnlyCollection<string> AvailableThemes => [];
        public bool ApplyByName(string name) => false;
        public void SaveCurrent() { }
    }

    private sealed class MutableFakeThemeService : IThemeService
    {
        public ThemeDefinition Current { get; set; } = new();
        public string? CurrentThemeName => null;
        public Observable<ThemeDefinition> Changes => Observable.Empty<ThemeDefinition>();
        public IReadOnlyCollection<string> AvailableThemes => [];
        public bool ApplyByName(string name) => false;
        public void SaveCurrent() { }
    }
}
