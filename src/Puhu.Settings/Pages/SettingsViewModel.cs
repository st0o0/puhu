using Puhu.Plugin;
using R3;
using Termina.Reactive;

namespace Puhu.Settings.Pages;

public enum SettingsView
{
    Themes,
    Refresh,
    Tabs
}

public sealed class SettingsViewModel : ReactiveViewModel
{
    private readonly IThemeService _themeService;
    private readonly IRefreshController _refreshController;
    private readonly ITabOrderService _tabOrderService;

    public IReadOnlyList<string> Themes { get; }
    public ReactiveProperty<int> SelectedIndex { get; }
    public ReactiveProperty<string?> SavedTheme { get; }

    public ReactiveProperty<SettingsView> ActiveView { get; } = new(SettingsView.Themes);

    public ReactiveProperty<int> TabSelectedIndex { get; } = new(0);

    public IReadOnlyList<TabDescriptor> Tabs => _tabOrderService.Tabs;

    public Observable<Unit> TabOrderChanged => _tabOrderService.Changed;

    public void MoveTabSelection(int delta)
    {
        if (Tabs.Count == 0)
        {
            return;
        }

        TabSelectedIndex.Value = Math.Clamp(TabSelectedIndex.Value + delta, 0, Tabs.Count - 1);
    }

    public void MoveTab(int delta)
    {
        if (Tabs.Count == 0)
        {
            return;
        }

        var index = TabSelectedIndex.Value;
        _tabOrderService.Move(index, delta);
        TabSelectedIndex.Value = Math.Clamp(index + delta, 0, Tabs.Count - 1);
    }

    public IReadOnlyList<TimeSpan> RefreshSteps => _refreshController.Steps;

    public int RefreshSelectedIndex
    {
        get
        {
            for (var i = 0; i < _refreshController.Steps.Count; i++)
            {
                if (_refreshController.Steps[i] == _refreshController.Interval.CurrentValue)
                {
                    return i;
                }
            }

            return 0;
        }
    }

    public bool IsPaused => _refreshController.IsPaused.CurrentValue;

    public SettingsViewModel(
        IThemeService themeService,
        IRefreshController refreshController,
        ITabOrderService tabOrderService)
    {
        _themeService = themeService;
        _refreshController = refreshController;
        _tabOrderService = tabOrderService;

        Themes = themeService.AvailableThemes
            .OrderBy(t => t, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var currentIndex = 0;
        if (_themeService.CurrentThemeName is { } name)
        {
            var idx = ((List<string>)Themes).FindIndex(t => t.Equals(name, StringComparison.OrdinalIgnoreCase));
            currentIndex = Math.Max(0, idx);
        }

        SelectedIndex = new ReactiveProperty<int>(currentIndex);
        SavedTheme = new ReactiveProperty<string?>(_themeService.CurrentThemeName);
    }

    public void MoveSelection(int delta)
    {
        if (Themes.Count == 0)
        {
            return;
        }

        var next = Math.Clamp(SelectedIndex.Value + delta, 0, Themes.Count - 1);
        SelectedIndex.Value = next;
        _themeService.ApplyByName(Themes[next]);
    }

    public void SaveSelected()
    {
        if (Themes.Count == 0)
        {
            return;
        }

        _themeService.SaveCurrent();
        SavedTheme.Value = Themes[SelectedIndex.Value];
    }

    public void MoveRefreshSelection(int delta)
    {
        var next = Math.Clamp(RefreshSelectedIndex + delta, 0, _refreshController.Steps.Count - 1);
        _refreshController.SetInterval(_refreshController.Steps[next]);
    }

    public override void OnActivated() { }

    public override void Dispose()
    {
        SelectedIndex.Dispose();
        SavedTheme.Dispose();
        ActiveView.Dispose();
        TabSelectedIndex.Dispose();
        base.Dispose();
    }
}
