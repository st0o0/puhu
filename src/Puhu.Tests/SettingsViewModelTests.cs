using Puhu.Plugin;
using Puhu.Settings.Pages;
using R3;

namespace Puhu.Tests;

public sealed class SettingsViewModelTests
{
    [Fact]
    public void Ctor_SelectsCurrentTheme()
    {
        var service = new FakeThemeService(["alpha", "beta", "gamma"]) { Name = "beta" };

        var vm = CreateVm(service);

        Assert.Equal(1, vm.SelectedIndex.Value);
    }

    [Fact]
    public void MoveSelection_AppliesThemeAsLivePreview()
    {
        var service = new FakeThemeService(["alpha", "beta"]);
        var vm = CreateVm(service);

        vm.MoveSelection(1);

        Assert.Equal("beta", service.LastApplied);
        Assert.Equal(1, vm.SelectedIndex.Value);
    }

    [Fact]
    public void MoveSelection_ClampsAtEnds()
    {
        var service = new FakeThemeService(["alpha", "beta"]);
        var vm = CreateVm(service);

        vm.MoveSelection(-1);
        Assert.Equal(0, vm.SelectedIndex.Value);

        vm.MoveSelection(5);
        Assert.Equal(1, vm.SelectedIndex.Value);
    }

    [Fact]
    public void SaveSelected_PersistsAndMarksSaved()
    {
        var service = new FakeThemeService(["alpha", "beta"]);
        var vm = CreateVm(service);
        vm.MoveSelection(1);

        vm.SaveSelected();

        Assert.True(service.Saved);
        Assert.Equal("beta", vm.SavedTheme.Value);
    }

    [Fact]
    public void EmptyThemes_MoveAndSave_DoNotThrow()
    {
        var service = new FakeThemeService([]);
        var vm = CreateVm(service);

        vm.MoveSelection(1);
        vm.SaveSelected();

        Assert.Equal(0, vm.SelectedIndex.Value);
    }

    [Fact]
    public void MoveRefreshSelection_SetsIntervalToStep()
    {
        var controller = new FakeRefreshController(); // starts at 1s = Steps[2]
        var vm = CreateVm(controller: controller);

        vm.MoveRefreshSelection(-1);

        Assert.Equal(TimeSpan.FromMilliseconds(500), controller.Interval.CurrentValue);
    }

    [Fact]
    public void MoveRefreshSelection_ClampsAtEnds()
    {
        var controller = new FakeRefreshController();
        var vm = CreateVm(controller: controller);

        vm.MoveRefreshSelection(-10);
        Assert.Equal(TimeSpan.FromMilliseconds(250), controller.Interval.CurrentValue);

        vm.MoveRefreshSelection(10);
        Assert.Equal(TimeSpan.FromMilliseconds(4000), controller.Interval.CurrentValue);
    }

    [Fact]
    public void RefreshSelectedIndex_FollowsControllerInterval()
    {
        var controller = new FakeRefreshController();
        var vm = CreateVm(controller: controller);

        controller.SetInterval(TimeSpan.FromMilliseconds(250));

        Assert.Equal(0, vm.RefreshSelectedIndex);
    }

    private static SettingsViewModel CreateVm(
        FakeThemeService? themeService = null,
        FakeRefreshController? controller = null)
    {
        themeService ??= new FakeThemeService([]);
        controller ??= new FakeRefreshController();
        return new SettingsViewModel(themeService, controller);
    }

    private sealed class FakeThemeService(IReadOnlyList<string> themes) : IThemeService
    {
        public string? Name { get; set; }
        public string? LastApplied { get; private set; }
        public bool Saved { get; private set; }

        public ThemeDefinition Current { get; } = new();
        public string? CurrentThemeName => LastApplied ?? Name;
        public Observable<ThemeDefinition> Changes => Observable.Empty<ThemeDefinition>();
        public IReadOnlyCollection<string> AvailableThemes => themes.ToList();

        public bool ApplyByName(string name)
        {
            LastApplied = name;
            return true;
        }

        public void SaveCurrent() => Saved = true;
    }
}
