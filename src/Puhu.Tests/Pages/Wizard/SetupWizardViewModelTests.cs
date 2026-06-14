using Puhu.Plugin;
using Puhu.Settings;
using Puhu.Settings.Pages;
using Puhu.Tests.Fakes;
using R3;

namespace Puhu.Tests.Pages.Wizard;

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
        var store = new FakeSettingsStore();
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
        var store = new FakeSettingsStore();
        string? navigated = null;
        var vm = CreateVm(store: store, tabs: new FakeTabOrderService(("System", "/system")));
        vm.NavigateTo = r => navigated = r;

        vm.Skip();

        Assert.True(store.IsWizardComplete());
        Assert.Equal("/system", navigated);
    }

    private static SetupWizardViewModel CreateVm(
        FakeSettingsStore? store = null,
        FakeThemeService? theme = null,
        FakeRefreshController? refresh = null,
        FakeTabOrderService? tabs = null)
    {
        return new SetupWizardViewModel(
            store ?? new FakeSettingsStore(),
            theme ?? new FakeThemeService(["alpha"]),
            refresh ?? new FakeRefreshController(),
            tabs ?? new FakeTabOrderService(("System", "/system")));
    }
}
