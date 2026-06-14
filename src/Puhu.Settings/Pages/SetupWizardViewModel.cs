using Puhu.Plugin;
using Termina.Reactive;

namespace Puhu.Settings.Pages;

public enum SetupStep
{
    Welcome,
    Theme,
    Refresh,
    TabOrder,
    Plugins,
    Done
}

/// <summary>
/// State and finish/skip logic for the first-run setup wizard. Rendering and
/// input live in <see cref="SetupWizardPage"/>.
/// </summary>
public sealed class SetupWizardViewModel : ReactiveViewModel
{
    private readonly ISettingsStore _settings;
    private readonly IThemeService _themeService;
    private readonly IRefreshController _refreshController;
    private readonly ITabOrderService _tabOrderService;

    public IReadOnlyList<string> Themes { get; }
    public int ThemeIndex { get; private set; }

    public IReadOnlyList<TimeSpan> RefreshSteps => _refreshController.Steps;

    public IReadOnlyList<TabDescriptor> Tabs => _tabOrderService.Tabs;
    public int TabIndex { get; private set; }

    public bool OpenMarketplaceAfter { get; private set; }

    /// <summary>Navigation hook. Defaults to the framework navigator; overridable in tests.</summary>
    public Action<string> NavigateTo { get; set; }

    public SetupWizardViewModel(
        ISettingsStore settings,
        IThemeService themeService,
        IRefreshController refreshController,
        ITabOrderService tabOrderService)
    {
        _settings = settings;
        _themeService = themeService;
        _refreshController = refreshController;
        _tabOrderService = tabOrderService;

        Themes = themeService.AvailableThemes
            .OrderBy(t => t, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (_themeService.CurrentThemeName is { } name)
        {
            var idx = ((List<string>)Themes).FindIndex(t => t.Equals(name, StringComparison.OrdinalIgnoreCase));
            ThemeIndex = Math.Max(0, idx);
        }

        NavigateTo = route => Navigate(route);
    }

    public int RefreshIndex
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

    public void MoveTheme(int delta)
    {
        if (Themes.Count == 0)
        {
            return;
        }

        ThemeIndex = Math.Clamp(ThemeIndex + delta, 0, Themes.Count - 1);
        _themeService.ApplyByName(Themes[ThemeIndex]);
    }

    public void MoveRefresh(int delta)
    {
        var next = Math.Clamp(RefreshIndex + delta, 0, _refreshController.Steps.Count - 1);
        _refreshController.SetInterval(_refreshController.Steps[next]);
    }

    public void MoveTabSelection(int delta)
    {
        if (Tabs.Count == 0)
        {
            return;
        }

        TabIndex = Math.Clamp(TabIndex + delta, 0, Tabs.Count - 1);
    }

    public void MoveTab(int delta)
    {
        if (Tabs.Count == 0)
        {
            return;
        }

        _tabOrderService.Move(TabIndex, delta);
        TabIndex = Math.Clamp(TabIndex + delta, 0, Tabs.Count - 1);
    }

    public void ToggleOpenMarketplace() => OpenMarketplaceAfter = !OpenMarketplaceAfter;

    public void Finish()
    {
        if (Themes.Count > 0)
        {
            _themeService.SaveCurrent();
        }

        Complete();
    }

    public void Skip() => Complete();

    private void Complete()
    {
        SetupWizardState.MarkComplete(_settings);

        var target = OpenMarketplaceAfter
            ? "/marketplace"
            : _tabOrderService.Tabs.FirstOrDefault()?.Route ?? "/marketplace";

        NavigateTo(target);
    }
}
