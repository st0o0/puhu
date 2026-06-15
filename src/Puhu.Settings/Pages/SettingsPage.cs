using Puhu.Plugin;
using Puhu.Plugin.Nodes;
using R3;
using Termina.Layout;
using Termina.Reactive;
using Termina.Rendering;

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
        SettingsView.Themes => ["↑↓:Theme", "Enter:Save"],
        SettingsView.Tabs => ["↑↓:Select", "⇧↑↓:Move"],
        SettingsView.Setup => ["Enter:Run"],
        _ => ["↑↓:Rate"],
    };

    public override ILayoutNode BuildLayout()
    {
        var theme = _themeService.Current;

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
            new SubNavSeparatorNode(theme.Accent).Height(1),
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
        var themeRows = SettingsRows.ThemeRows(
            ViewModel.Themes, ViewModel.SelectedIndex.Value, ViewModel.SavedTheme.Value, theme);

        var themeList = new PanelNode()
            .WithBorder(BorderStyle.Rounded)
            .WithBorderColor(theme.Accent)
            .WithTitle("◆ Theme")
            .WithTitleColor(theme.PanelTitle)
            .WithPadding(1)
            .WithContent(Layouts.Vertical(themeRows.ToArray()));

        var palette = new PanelNode()
            .WithBorder(BorderStyle.Rounded)
            .WithBorderColor(theme.Accent)
            .WithTitle("◨ Preview")
            .WithTitleColor(theme.PanelTitle)
            .WithPadding(1)
            .WithContent(new ThemePaletteNode(theme));

        return Layouts.Horizontal(
            themeList.WidthPercent(50),
            palette.WidthPercent(50));
    }

    private LayoutNode BuildTabsView()
    {
        var theme = _themeService.Current;

        var hint = new TextNode("Drag tabs with Shift+↑↓ to reorder. Changes apply immediately.")
            .WithForeground(theme.TextDim).Height(1);
        var spacer = Layouts.Empty().Height(1);

        var header = Layouts.Horizontal(
            new TextNode("  #").WithForeground(theme.Foreground).Bold().WidthAuto(min: 5),
            new TextNode("Tab").WithForeground(theme.Foreground).Bold().WidthPercent(50),
            new TextNode("Plugin").WithForeground(theme.Foreground).Bold().WidthFill()
        ).Height(1);

        var separator = new RuleNode(theme.Border).Height(1);
        var rows = new List<ILayoutNode> { hint, spacer, header, separator };

        for (var i = 0; i < ViewModel.Tabs.Count; i++)
        {
            var tab = ViewModel.Tabs[i];
            var isSelected = i == ViewModel.TabSelectedIndex.Value;
            var marker = isSelected ? "▸" : " ";

            rows.Add(Layouts.Horizontal(
                new TextNode($"{marker} {i + 1}").WithForeground(isSelected ? theme.Foreground : theme.TextDim).WidthAuto(min: 5),
                new TextNode(tab.Label.ToLowerInvariant()).WithForeground(isSelected ? theme.Foreground : theme.TextDim).WidthPercent(50),
                new TextNode("plugin").WithForeground(theme.TextDim).WidthFill()
            ).Height(1));
        }

        if (ViewModel.Tabs.Count == 0)
            rows.Add(new TextNode("no tabs registered").WithForeground(theme.TextDim).Height(1));

        return new PanelNode()
            .WithBorder(BorderStyle.Rounded)
            .WithBorderColor(theme.Accent)
            .WithTitle("☰ Tab Order")
            .WithTitleColor(theme.PanelTitle)
            .WithPadding(1)
            .WithContent(Layouts.Vertical(rows.ToArray()));
    }

    private LayoutNode BuildSetupView()
    {
        var theme = _themeService.Current;

        var wizardContent = Layouts.Vertical(
            new TextNode("Re-run the initial setup wizard to reconfigure your")
                .WithForeground(theme.TextDim).Height(1),
            new TextNode("dashboard from scratch.")
                .WithForeground(theme.TextDim).Height(1),
            Layouts.Empty().Height(1),
            new TextNode("⚠ This will walk you through theme, refresh rate,")
                .WithForeground(theme.Warning).Height(1),
            new TextNode("  tab order, and plugin configuration.")
                .WithForeground(theme.Warning).Height(1),
            Layouts.Empty().Height(1),
            Layouts.Horizontal(
                new TextNode("Press ").WithForeground(theme.TextDim).WidthAuto(),
                new TextNode("Enter").WithForeground(theme.Foreground).Bold().WidthAuto(),
                new TextNode(" to launch wizard").WithForeground(theme.TextDim).WidthFill()
            ).Height(1)
        );

        var wizardPanel = new PanelNode()
            .WithBorder(BorderStyle.Rounded)
            .WithBorderColor(theme.Accent)
            .WithTitle("⚙ Setup Wizard")
            .WithTitleColor(theme.PanelTitle)
            .WithPadding(1)
            .WithContent(wizardContent);

        var puhuVersion = typeof(SettingsPage).Assembly.GetName().Version;
        var terminaVersion = typeof(Layouts).Assembly.GetName().Version;

        var aboutContent = Layouts.Vertical(
            new KeyValueRowNode("puhu", $"v{puhuVersion?.ToString(3) ?? "?"}", labelWidth: 12, labelColor: theme.TextDim, valueColor: theme.Foreground),
            new KeyValueRowNode("termina", $"v{terminaVersion?.ToString(3) ?? "?"}", labelWidth: 12, labelColor: theme.TextDim, valueColor: theme.Foreground),
            new KeyValueRowNode("plugins", $"{ViewModel.Tabs.Count} loaded", labelWidth: 12, labelColor: theme.TextDim, valueColor: theme.Foreground)
        );

        var aboutPanel = new PanelNode()
            .WithBorder(BorderStyle.Rounded)
            .WithBorderColor(theme.Accent)
            .WithTitle("ℹ About")
            .WithTitleColor(theme.PanelTitle)
            .WithPadding(1)
            .WithContent(aboutContent);

        return Layouts.Vertical(wizardPanel, aboutPanel);
    }

    private LayoutNode BuildRefreshView()
    {
        var theme = _themeService.Current;

        var descNode = new TextNode("How often the dashboard fetches new data from plugins.")
            .WithForeground(theme.TextDim).Height(1);
        var spacer = Layouts.Empty().Height(1);
        var rateRows = SettingsRows.RefreshRows(ViewModel.RefreshSteps, ViewModel.RefreshSelectedIndex, theme);
        var rateContent = new List<ILayoutNode> { descNode, spacer };
        rateContent.AddRange(rateRows);

        var ratePanel = new PanelNode()
            .WithBorder(BorderStyle.Rounded)
            .WithBorderColor(theme.Accent)
            .WithTitle("⏱ Refresh Rate")
            .WithTitleColor(theme.PanelTitle)
            .WithPadding(1)
            .WithContent(Layouts.Vertical(rateContent.ToArray()));

        var isPaused = ViewModel.IsPaused;
        var statusBadge = isPaused
            ? new BadgeNode("PAUSED", theme.SelectionText, theme.Warning, icon: "⏸")
            : new BadgeNode("RUNNING", theme.SelectionText, theme.Success, icon: "▶");

        var pauseContent = Layouts.Horizontal(
            new TextNode("Status:  ").WithForeground(theme.TextDim).WidthAuto(),
            statusBadge.Height(1),
            new TextNode("   Press Space to pause").WithForeground(theme.TextDim).WidthFill()
        );

        var pausePanel = new PanelNode()
            .WithBorder(BorderStyle.Rounded)
            .WithBorderColor(theme.Accent)
            .WithTitle("⏸ Pause Control")
            .WithTitleColor(theme.PanelTitle)
            .WithPadding(1)
            .WithContent(pauseContent);

        return Layouts.Vertical(ratePanel, pausePanel);
    }
}
