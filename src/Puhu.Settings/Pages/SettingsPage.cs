using Puhu.Plugin;
using Puhu.Plugin.Nodes;
using R3;
using Termina.Layout;
using Termina.Reactive;

namespace Puhu.Settings.Pages;

public sealed class SettingsPage : ReactivePage<SettingsViewModel>, IKeyHintProvider
{
    private readonly ITabNavigator _tabNavigator;
    private readonly IThemeService _themeService;
    private readonly IRefreshController _refreshController;
    private SubNavNode<SettingsView>? _subNav;
    private KeyedDynamicLayoutNode<SettingsView>? _viewSwitcher;

    public SettingsPage(ITabNavigator tabNavigator, IThemeService themeService, IRefreshController refreshController)
    {
        _tabNavigator = tabNavigator;
        _themeService = themeService;
        _refreshController = refreshController;
    }

    protected override void OnBound()
    {
        _subNav = new SubNavNode<SettingsView>(
            ViewModel.ActiveView,
            KeyBindings,
            _themeService,
            (ConsoleKey.D1, "Themes", SettingsView.Themes),
            (ConsoleKey.D2, "Refresh", SettingsView.Refresh));

        _viewSwitcher = Layouts.KeyedDynamic(
            () => ViewModel.ActiveView.Value,
            view => view switch
            {
                SettingsView.Themes => BuildThemesView(),
                SettingsView.Refresh => BuildRefreshView(),
                _ => Layouts.Empty()
            });
    }

    public string[] GetKeyHints() => ViewModel.ActiveView.Value switch
    {
        SettingsView.Themes => ["↑↓:Theme", "Enter:Save", "Esc:Quit", "Tab:Switch"],
        _ => ["↑↓:Rate", "p:Pause", "Esc:Quit", "Tab:Switch"],
    };

    public override ILayoutNode BuildLayout()
    {
        return Layouts.Vertical(
            _subNav!.Height(1),
            _viewSwitcher!.Fill());
    }

    public override void OnNavigatedTo()
    {
        base.OnNavigatedTo();

        KeyBindings.RegisterGlobalKeys(
            () => ViewModel.RequestShutdown(),
            path => Navigate(path),
            _tabNavigator,
            _refreshController);

        KeyBindings.Register(ConsoleKey.UpArrow, () =>
        {
            if (ViewModel.ActiveView.Value == SettingsView.Themes) ViewModel.MoveSelection(-1);
            else ViewModel.MoveRefreshSelection(-1);
        });
        KeyBindings.Register(ConsoleKey.DownArrow, () =>
        {
            if (ViewModel.ActiveView.Value == SettingsView.Themes) ViewModel.MoveSelection(1);
            else ViewModel.MoveRefreshSelection(1);
        });
        KeyBindings.Register(ConsoleKey.Enter, () =>
        {
            if (ViewModel.ActiveView.Value == SettingsView.Themes) ViewModel.SaveSelected();
        });

        ViewModel.SelectedIndex.Subscribe(_ => InvalidateLayout()).DisposeWith(Subscriptions);
        ViewModel.SavedTheme.Subscribe(_ => InvalidateLayout()).DisposeWith(Subscriptions);
        ViewModel.ActiveView
            .Subscribe(_ =>
            {
                _viewSwitcher?.Invalidate();
                InvalidateLayout();
            })
            .DisposeWith(Subscriptions);
        _refreshController.Interval.Skip(1).Subscribe(_ => InvalidateLayout()).DisposeWith(Subscriptions);
        _refreshController.IsPaused.Skip(1).Subscribe(_ => InvalidateLayout()).DisposeWith(Subscriptions);
    }

    private ILayoutNode BuildThemesView()
    {
        var theme = _themeService.Current;
        var rows = new List<ILayoutNode>
        {
            new TextNode("themes").WithForeground(theme.TextDim).Bold().Height(1),
        };

        for (var i = 0; i < ViewModel.Themes.Count; i++)
        {
            var name = ViewModel.Themes[i];
            var isSelected = i == ViewModel.SelectedIndex.Value;
            var isSaved = string.Equals(name, ViewModel.SavedTheme.Value, StringComparison.OrdinalIgnoreCase);
            var marker = isSelected ? "▸" : " ";
            var suffix = isSaved ? "  ●" : "";

            rows.Add(new TextNode($"{marker} {name}{suffix}")
                .WithForeground(isSelected ? theme.Foreground : theme.TextDim)
                .Height(1));
        }

        if (ViewModel.Themes.Count == 0)
        {
            rows.Add(new TextNode("no themes found").WithForeground(theme.TextDim).Height(1));
        }

        return Layouts.Vertical(rows.ToArray());
    }

    private ILayoutNode BuildRefreshView()
    {
        var theme = _themeService.Current;
        var rows = new List<ILayoutNode>
        {
            new TextNode("refresh rate").WithForeground(theme.TextDim).Bold().Height(1),
        };

        for (var i = 0; i < ViewModel.RefreshSteps.Count; i++)
        {
            var isSelected = i == ViewModel.RefreshSelectedIndex;
            var marker = isSelected ? "▸" : " ";
            rows.Add(new TextNode($"{marker} {IntervalFormat.Format(ViewModel.RefreshSteps[i])}")
                .WithForeground(isSelected ? theme.Foreground : theme.TextDim)
                .Height(1));
        }

        rows.Add(Layouts.Empty().Height(1));
        rows.Add(new TextNode($"paused: {(ViewModel.IsPaused ? "yes" : "no")}  (p toggles)")
            .WithForeground(ViewModel.IsPaused ? theme.Warning : theme.TextDim)
            .Height(1));

        return Layouts.Vertical(rows.ToArray());
    }
}
