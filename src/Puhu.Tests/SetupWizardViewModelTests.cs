using Puhu.Plugin;
using Puhu.Settings;
using Puhu.Settings.Pages;
using R3;

namespace Puhu.Tests;

public sealed class SetupWizardViewModelTests
{
    [Fact]
    public void MoveTheme_AppliesPreviewAndClamps()
    {
        var theme = new FakeThemeService(["alpha", "beta"]);
        var vm = CreateVm(theme: theme);

        vm.MoveTheme(1);
        Assert.Equal("beta", theme.LastApplied);
        Assert.Equal(1, vm.ThemeIndex);

        vm.MoveTheme(5);
        Assert.Equal(1, vm.ThemeIndex);
    }

    [Fact]
    public void Finish_MarksCompleteSavesThemeAndNavigatesToFirstTab()
    {
        var store = new FakeStore();
        var theme = new FakeThemeService(["alpha", "beta"]);
        var tabs = new FakeTabOrderService(("System", "/system"), ("Settings", "/settings"));
        string? navigated = null;
        var vm = CreateVm(store: store, theme: theme, tabs: tabs);
        vm.NavigateTo = r => navigated = r;

        vm.Finish();

        Assert.True(store.IsWizardComplete());
        Assert.True(theme.Saved);
        Assert.Equal("/system", navigated);
    }

    [Fact]
    public void Finish_WhenOpenMarketplaceToggled_NavigatesToMarketplace()
    {
        var tabs = new FakeTabOrderService(("System", "/system"));
        string? navigated = null;
        var vm = CreateVm(tabs: tabs);
        vm.NavigateTo = r => navigated = r;

        vm.ToggleOpenMarketplace();
        vm.Finish();

        Assert.Equal("/marketplace", navigated);
    }

    [Fact]
    public void Skip_MarksCompleteAndNavigates()
    {
        var store = new FakeStore();
        string? navigated = null;
        var vm = CreateVm(store: store, tabs: new FakeTabOrderService(("System", "/system")));
        vm.NavigateTo = r => navigated = r;

        vm.Skip();

        Assert.True(store.IsWizardComplete());
        Assert.Equal("/system", navigated);
    }

    private static SetupWizardViewModel CreateVm(
        FakeStore? store = null,
        FakeThemeService? theme = null,
        FakeRefreshController? refresh = null,
        FakeTabOrderService? tabs = null)
    {
        return new SetupWizardViewModel(
            store ?? new FakeStore(),
            theme ?? new FakeThemeService(["alpha"]),
            refresh ?? new FakeRefreshController(),
            tabs ?? new FakeTabOrderService(("System", "/system")));
    }

    private sealed class FakeStore : ISettingsStore
    {
        private readonly Dictionary<string, object?> _data = new();
        public T? Get<T>(string key) => _data.TryGetValue(key, out var v) ? (T?)v : default;
        public void Set<T>(string key, T value) => _data[key] = value;
        public Observable<T> Observe<T>(string key) => Observable.Empty<T>();
        public void Remove(string key) => _data.Remove(key);
    }

    private sealed class FakeThemeService(IReadOnlyList<string> themes) : IThemeService
    {
        public string? LastApplied { get; private set; }
        public bool Saved { get; private set; }
        public ThemeDefinition Current { get; } = new();
        public string? CurrentThemeName => LastApplied;
        public Observable<ThemeDefinition> Changes => Observable.Empty<ThemeDefinition>();
        public IReadOnlyCollection<string> AvailableThemes => themes.ToList();
        public bool ApplyByName(string name) { LastApplied = name; return true; }
        public void SaveCurrent() => Saved = true;
    }
}
