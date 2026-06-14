using System.Reflection;
using Puhu.Plugin;
using Puhu.Settings.Pages;
using R3;
using Termina.Layout;
using Termina.Rendering;
using Termina.Reactive;

namespace Puhu.Tests;

/// <summary>
/// Regression tests for the stale-view bug: SettingsPage used to cache its sub-views
/// in a KeyedDynamicLayoutNode, whose per-key cache never re-invokes the factory —
/// so the ▸ marker and the paused line froze after the first paint.
/// These tests prove successive BuildLayout() calls reflect current state.
/// </summary>
public sealed class SettingsPageRenderTests
{
    private const int Width = 40;
    private const int Height = 12;

    [Fact]
    public void RefreshView_MarkerFollowsIntervalAcrossRebuilds()
    {
        var controller = new FakeRefreshController(); // starts at 1s
        var (page, vm) = CreateBoundPage(controller: controller);
        vm.ActiveView.Value = SettingsView.Refresh;

        var first = RenderLayout(page.BuildLayout());
        Assert.Contains("▸ 1s", first);

        controller.SetInterval(TimeSpan.FromMilliseconds(250));

        var second = RenderLayout(page.BuildLayout());
        Assert.Contains("▸ 250ms", second);
        Assert.DoesNotContain("▸ 1s", second);
    }

    [Fact]
    public void RefreshView_PausedLineFollowsControllerAcrossRebuilds()
    {
        var controller = new FakeRefreshController();
        var (page, vm) = CreateBoundPage(controller: controller);
        vm.ActiveView.Value = SettingsView.Refresh;

        var first = RenderLayout(page.BuildLayout());
        Assert.Contains("paused: no", first);

        controller.TogglePause();

        var second = RenderLayout(page.BuildLayout());
        Assert.Contains("paused: yes", second);
    }

    [Fact]
    public void ThemesView_MarkerFollowsSelectionAcrossRebuilds()
    {
        var (page, vm) = CreateBoundPage(themes: ["alpha", "beta"]);

        var first = RenderLayout(page.BuildLayout());
        Assert.Contains("▸ alpha", first);

        vm.MoveSelection(1);

        var second = RenderLayout(page.BuildLayout());
        Assert.Contains("▸ beta", second);
        Assert.DoesNotContain("▸ alpha", second);
    }

    [Fact]
    public void BuildLayout_SwitchingViews_RendersActiveViewContent()
    {
        var (page, vm) = CreateBoundPage(themes: ["alpha"]);

        var themesView = RenderLayout(page.BuildLayout());
        Assert.Contains("themes", themesView);

        vm.ActiveView.Value = SettingsView.Refresh;

        var refreshView = RenderLayout(page.BuildLayout());
        Assert.Contains("refresh rate", refreshView);
        Assert.DoesNotContain("▸ alpha", refreshView);
    }

    private static (SettingsPage Page, SettingsViewModel ViewModel) CreateBoundPage(
        IReadOnlyList<string>? themes = null,
        FakeRefreshController? controller = null,
        FakeTabOrderService? tabOrder = null)
    {
        var themeService = new FakeThemeService(themes ?? []);
        controller ??= new FakeRefreshController();

        var page = new SettingsPage(new FakeTabNavigator(), themeService, controller);
        var vm = new SettingsViewModel(themeService, controller, tabOrder ?? new FakeTabOrderService());

        // Bind is internal to Termina (normally invoked by the framework during
        // page initialization) — reflection is the only way to bind in isolation.
        var bind = typeof(ReactivePage<SettingsViewModel>)
            .GetMethod("Bind", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("ReactivePage<T>.Bind not found.");
        bind.Invoke(page, [vm]);

        return (page, vm);
    }

    private static string RenderLayout(ILayoutNode layout)
    {
        var ctx = new RenderTestContext(Width, Height);
        layout.Measure(new Size(Width, Height));
        layout.Render(ctx, new Rect(0, 0, Width, Height));

        var rows = new string[Height];
        for (var y = 0; y < Height; y++)
        {
            rows[y] = ctx.Row(y);
        }

        return string.Join('\n', rows);
    }

    private sealed class FakeTabNavigator : ITabNavigator
    {
        public bool HasTabs => false;
        public void CycleTab(Action<string> navigate, int delta) { }
    }

    private sealed class FakeThemeService(IReadOnlyList<string> themes) : IThemeService
    {
        public ThemeDefinition Current { get; } = new();
        public string? CurrentThemeName { get; private set; }
        public Observable<ThemeDefinition> Changes => Observable.Empty<ThemeDefinition>();
        public IReadOnlyCollection<string> AvailableThemes => themes.ToList();

        public bool ApplyByName(string name)
        {
            CurrentThemeName = name;
            return true;
        }

        public void SaveCurrent() { }
    }
}
