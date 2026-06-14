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
            (ConsoleKey.D2, "Refresh", SettingsView.Refresh),
            (ConsoleKey.D3, "Tabs", SettingsView.Tabs),
            (ConsoleKey.D4, "Setup", SettingsView.Setup));
    }

    public string[] GetKeyHints() => ViewModel.ActiveView.Value switch
    {
        SettingsView.Themes => ["↑↓:Theme", "Enter:Save", "Esc:Quit", "Tab:Switch"],
        SettingsView.Tabs => ["↑↓:Select", "⇧↑↓:Move", "Esc:Quit", "Tab:Switch"],
        SettingsView.Setup => ["Enter:Run", "Esc:Quit", "Tab:Switch"],
        _ => ["↑↓:Rate", "p:Pause", "Esc:Quit", "Tab:Switch"],
    };

    public override ILayoutNode BuildLayout()
    {
        // Views are rebuilt fresh on every layout pass: they are static TextNode
        // snapshots of ViewModel state, so caching them (e.g. via KeyedDynamic)
        // would freeze selection markers and the paused line after first paint.
        LayoutNode activeView = ViewModel.ActiveView.Value switch
        {
            SettingsView.Themes => BuildThemesView(),
            SettingsView.Tabs => BuildTabsView(),
            SettingsView.Setup => BuildSetupView(),
            _ => BuildRefreshView(),
        };

        return Layouts.Vertical(
            _subNav!.Height(1),
            activeView.Fill());
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
            switch (ViewModel.ActiveView.Value)
            {
                case SettingsView.Themes: ViewModel.MoveSelection(-1); break;
                case SettingsView.Refresh: ViewModel.MoveRefreshSelection(-1); break;
                case SettingsView.Tabs: ViewModel.MoveTabSelection(-1); break;
            }
        });
        KeyBindings.Register(ConsoleKey.DownArrow, () =>
        {
            switch (ViewModel.ActiveView.Value)
            {
                case SettingsView.Themes: ViewModel.MoveSelection(1); break;
                case SettingsView.Refresh: ViewModel.MoveRefreshSelection(1); break;
                case SettingsView.Tabs: ViewModel.MoveTabSelection(1); break;
            }
        });
        KeyBindings.Register(ConsoleKey.UpArrow, ConsoleModifiers.Shift, () =>
        {
            if (ViewModel.ActiveView.Value == SettingsView.Tabs) ViewModel.MoveTab(-1);
        });
        KeyBindings.Register(ConsoleKey.DownArrow, ConsoleModifiers.Shift, () =>
        {
            if (ViewModel.ActiveView.Value == SettingsView.Tabs) ViewModel.MoveTab(1);
        });
        KeyBindings.Register(ConsoleKey.Enter, () =>
        {
            if (ViewModel.ActiveView.Value == SettingsView.Themes) ViewModel.SaveSelected();
            else if (ViewModel.ActiveView.Value == SettingsView.Setup) Navigate("/setup");
        });

        ViewModel.SelectedIndex.Subscribe(_ => InvalidateLayout()).DisposeWith(Subscriptions);
        ViewModel.SavedTheme.Subscribe(_ => InvalidateLayout()).DisposeWith(Subscriptions);
        ViewModel.ActiveView
            .Subscribe(_ => InvalidateLayout())
            .DisposeWith(Subscriptions);
        _refreshController.Interval.Skip(1).Subscribe(_ => InvalidateLayout()).DisposeWith(Subscriptions);
        _refreshController.IsPaused.Skip(1).Subscribe(_ => InvalidateLayout()).DisposeWith(Subscriptions);
        ViewModel.TabSelectedIndex.Subscribe(_ => InvalidateLayout()).DisposeWith(Subscriptions);
        ViewModel.TabOrderChanged.Subscribe(_ => InvalidateLayout()).DisposeWith(Subscriptions);
    }

    private LayoutNode BuildThemesView()
    {
        var theme = _themeService.Current;
        var rows = new List<ILayoutNode>
        {
            new TextNode("themes").WithForeground(theme.TextDim).Bold().Height(1),
        };
        rows.AddRange(SettingsRows.ThemeRows(
            ViewModel.Themes, ViewModel.SelectedIndex.Value, ViewModel.SavedTheme.Value, theme));

        return Layouts.Vertical(rows.ToArray());
    }

    private LayoutNode BuildTabsView()
    {
        var theme = _themeService.Current;
        var rows = new List<ILayoutNode>
        {
            new TextNode("tab order").WithForeground(theme.TextDim).Bold().Height(1),
        };

        for (var i = 0; i < ViewModel.Tabs.Count; i++)
        {
            var isSelected = i == ViewModel.TabSelectedIndex.Value;
            var marker = isSelected ? "▸" : " ";
            rows.Add(new TextNode($"{marker} {ViewModel.Tabs[i].Label.ToLowerInvariant()}")
                .WithForeground(isSelected ? theme.Foreground : theme.TextDim)
                .Height(1));
        }

        if (ViewModel.Tabs.Count == 0)
        {
            rows.Add(new TextNode("no tabs").WithForeground(theme.TextDim).Height(1));
        }

        return Layouts.Vertical(rows.ToArray());
    }

    private LayoutNode BuildSetupView()
    {
        var theme = _themeService.Current;
        return Layouts.Vertical(
            new TextNode("setup").WithForeground(theme.TextDim).Bold().Height(1),
            new TextNode("▸ re-run setup wizard").WithForeground(theme.Foreground).Height(1));
    }

    private LayoutNode BuildRefreshView()
    {
        var theme = _themeService.Current;
        var rows = new List<ILayoutNode>
        {
            new TextNode("refresh rate").WithForeground(theme.TextDim).Bold().Height(1),
        };
        rows.AddRange(SettingsRows.RefreshRows(ViewModel.RefreshSteps, ViewModel.RefreshSelectedIndex, theme));

        rows.Add(Layouts.Empty().Height(1));
        rows.Add(new TextNode($"paused: {(ViewModel.IsPaused ? "yes" : "no")}  (p toggles)")
            .WithForeground(ViewModel.IsPaused ? theme.Warning : theme.TextDim)
            .Height(1));

        return Layouts.Vertical(rows.ToArray());
    }
}
