using Puhu.Marketplace.Models;
using Puhu.Plugin;
using Puhu.Plugin.Nodes;
using R3;
using Termina.Input;
using Termina.Layout;
using Termina.Notifications;
using Termina.Reactive;
using Termina.Rendering;
using Termina.Terminal;

namespace Puhu.Marketplace.Pages;

public sealed class MarketplacePage : ReactivePage<MarketplaceViewModel>, IKeyHintProvider
{
    private readonly IToastService _toastService;
    private readonly IThemeService _themeService;
    private readonly ITabNavigator _tabNavigator;
    private readonly IRefreshController _refreshController;
    private int _expandedIndex = -1;
    private SubNavNode<MarketplaceView>? _subNav;

    public MarketplacePage(IToastService toastService, IThemeService themeService, ITabNavigator tabNavigator, IRefreshController refreshController)
    {
        _toastService = toastService;
        _themeService = themeService;
        _tabNavigator = tabNavigator;
        _refreshController = refreshController;
        FocusPolicy = FocusPolicy.FirstFocusable;
    }

    protected override void OnBound()
    {
        _subNav = new SubNavNode<MarketplaceView>(
            ViewModel.ActiveView,
            KeyBindings,
            _themeService,
            (ConsoleKey.D1, "Browse", MarketplaceView.Browse),
            (ConsoleKey.D2, "Installed", MarketplaceView.Installed),
            (ConsoleKey.D3, "Sources", MarketplaceView.Sources));
    }

    public string[] GetKeyHints() => ViewModel.ActiveView.Value switch
    {
        MarketplaceView.Browse => ["↑↓:Navigate", "Enter:Expand", "i:Install", "r:Refresh"],
        MarketplaceView.Installed => ["↑↓:Navigate", "u:Update", "x:Uninstall", "c:Policy"],
        MarketplaceView.Sources => ["↑↓:Navigate", "a:Add", "x:Remove"],
        _ => []
    };

    public override ILayoutNode BuildLayout()
    {
        var theme = _themeService.Current;

        // Views are rebuilt fresh on every layout pass: they are static snapshots
        // of ViewModel state, so caching them (e.g. via KeyedDynamic) would freeze
        // selection markers and expansion state after the first paint.
        LayoutNode activeView = ViewModel.ActiveView.Value switch
        {
            MarketplaceView.Browse => BuildBrowseView(),
            MarketplaceView.Installed => BuildInstalledView(),
            MarketplaceView.Sources => BuildSourcesView(),
            _ => Layouts.Empty()
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

        // Navigation
        KeyBindings.Register(ConsoleKey.UpArrow, () =>
        {
            ViewModel.MoveSelection(-1);
            InvalidateLayout();
        });
        KeyBindings.Register(ConsoleKey.DownArrow, () =>
        {
            ViewModel.MoveSelection(1);
            InvalidateLayout();
        });

        // Expand/collapse (Browse view)
        KeyBindings.Register(ConsoleKey.Enter, () =>
        {
            if (ViewModel.ActiveView.Value == MarketplaceView.Browse)
            {
                _expandedIndex = _expandedIndex == ViewModel.SelectedIndex.Value
                    ? -1
                    : ViewModel.SelectedIndex.Value;
                InvalidateLayout();
            }
        });

        // Install action (Browse view)
        KeyBindings.Register(ConsoleKey.I, () =>
        {
            if (ViewModel.ActiveView.Value == MarketplaceView.Browse)
                ViewModel.HandleAction();
        });

        // Refresh
        KeyBindings.Register(ConsoleKey.R, () => ViewModel.Refresh());

        // Update (Installed view)
        KeyBindings.Register(ConsoleKey.U, () =>
        {
            if (ViewModel.ActiveView.Value == MarketplaceView.Installed)
                ViewModel.UpdateSelected();
        });

        // Uninstall (Installed view)
        KeyBindings.Register(ConsoleKey.X, () =>
        {
            if (ViewModel.ActiveView.Value == MarketplaceView.Installed)
                ViewModel.UninstallSelected();
        });

        // Cycle policy (Installed view)
        KeyBindings.Register(ConsoleKey.C, () =>
        {
            if (ViewModel.ActiveView.Value == MarketplaceView.Installed &&
                ViewModel.SelectedPlugin.Value is { } plugin)
                ViewModel.CyclePolicy(plugin.Id);
        });

        // Subscriptions
        ViewModel.ActiveView
            .Subscribe(_ => InvalidateLayout())
            .DisposeWith(Subscriptions);

        ViewModel.AvailablePlugins
            .Subscribe(_ => InvalidateLayout())
            .DisposeWith(Subscriptions);

        ViewModel.InstalledPlugins
            .Subscribe(_ => InvalidateLayout())
            .DisposeWith(Subscriptions);

        ViewModel.ActiveOperations
            .Subscribe(_ => InvalidateLayout())
            .DisposeWith(Subscriptions);

        ViewModel.Sources
            .Subscribe(_ => InvalidateLayout())
            .DisposeWith(Subscriptions);

        ViewModel.StatusMessage
            .Where(msg => msg is not null)
            .Subscribe(msg =>
            {
                var theme = _themeService.Current;
                var isError = msg!.StartsWith("Error:");
                _toastService.Show(msg, new ToastOptions(
                    Color: isError ? theme.Error : theme.Success,
                    Icon: isError ? "✗" : "✓"));
            })
            .DisposeWith(Subscriptions);
    }

    // --- Browse View ---

    private LayoutNode BuildBrowseView()
    {
        var theme = _themeService.Current;
        var plugins = ViewModel.AvailablePlugins.Value;
        if (plugins.Count == 0)
        {
            return new PanelNode()
                .WithBorder(BorderStyle.Rounded)
                .WithBorderColor(theme.Accent)
                .WithTitle("📦 Available Plugins")
                .WithTitleColor(theme.PanelTitle)
                .WithPadding(1)
                .WithContent(new TextNode("No plugins available. Press r to refresh.").WithForeground(theme.TextDim));
        }

        var rows = new List<ILayoutNode>();
        for (var i = 0; i < plugins.Count; i++)
        {
            var plugin = plugins[i];
            var isSelected = i == ViewModel.SelectedIndex.Value;
            var isExpanded = i == _expandedIndex;
            rows.AddRange(BuildBrowseRow(plugin, isSelected, isExpanded, theme));
            if (i < plugins.Count - 1) rows.Add(Layouts.Empty().Height(1));
        }

        return new PanelNode()
            .WithBorder(BorderStyle.Rounded)
            .WithBorderColor(theme.Accent)
            .WithTitle("📦 Available Plugins")
            .WithTitleColor(theme.PanelTitle)
            .WithPadding(1)
            .WithContent(Layouts.Vertical(rows.ToArray()));
    }

    private IReadOnlyList<ILayoutNode> BuildBrowseRow(PluginInfo plugin, bool isSelected, bool isExpanded, ThemeDefinition theme)
    {
        var marker = isExpanded ? "▾" : isSelected ? "▸" : " ";
        var activeOp = ViewModel.ActiveOperations.Value.GetValueOrDefault(plugin.Id);

        var nameAndVersion = new TextNode($"{marker} {plugin.Name}  {plugin.AvailableVersion}")
            .WithForeground(isSelected ? theme.Foreground : theme.TextDim).WidthFill();
        var authorNode = new TextNode($"by {plugin.Author}  ").WithForeground(theme.TextDim).WidthAuto();

        ILayoutNode actionNode = activeOp is not null
            ? new SpinnerNode().WithLabel(activeOp).WithSpinnerColor(theme.Success)
            : GetActionBadgeNode(plugin, theme);

        var header = Layouts.Horizontal(nameAndVersion, authorNode, actionNode).Height(1);
        var desc = new TextNode($"  {plugin.Description}").WithForeground(theme.TextDim).Height(1);
        var result = new List<ILayoutNode> { header, desc };

        if (isExpanded)
        {
            var tagText = string.Join("  ", plugin.Tags.Select(t => $"[{t}]"));
            var detailContent = Layouts.Vertical(
                new TextNode(plugin.Description).WithForeground(theme.TextDim).Height(1),
                Layouts.Empty().Height(1),
                Layouts.Horizontal(
                    new TextNode("Tags:  ").WithForeground(theme.TextDim).WidthAuto(),
                    new TextNode(tagText).WithForeground(theme.Accent).WidthFill()
                ).Height(1),
                new KeyValueRowNode("Type", plugin.Delivery.Type.ToString(), labelWidth: 10, labelColor: theme.TextDim, valueColor: theme.Foreground)
            );

            result.Add(new PanelNode()
                .WithBorder(BorderStyle.Rounded)
                .WithBorderColor(theme.Border)
                .WithPadding(1)
                .WithContent(detailContent));
        }

        return result;
    }

    private static ILayoutNode GetActionBadgeNode(PluginInfo plugin, ThemeDefinition theme) => plugin.Status switch
    {
        PluginStatus.NotInstalled => new BadgeNode("INSTALL", theme.SelectionText, theme.Accent, icon: "↓"),
        PluginStatus.Installed => new BadgeNode("INSTALLED", theme.SelectionText, theme.Success, icon: "✓"),
        PluginStatus.UpdateAvailable => new BadgeNode("UPDATE", theme.SelectionText, theme.Warning, icon: "↑"),
        _ => Layouts.Empty()
    };

    // --- Installed View ---

    private LayoutNode BuildInstalledView()
    {
        var theme = _themeService.Current;
        var plugins = ViewModel.InstalledPlugins.Value;

        if (plugins.Count == 0)
        {
            return new PanelNode()
                .WithBorder(BorderStyle.Rounded)
                .WithBorderColor(theme.Accent)
                .WithTitle("📋 Installed Plugins")
                .WithTitleColor(theme.PanelTitle)
                .WithPadding(1)
                .WithContent(new TextNode("No plugins installed.").WithForeground(theme.TextDim));
        }

        var header = Layouts.Horizontal(
            new TextNode("  Plugin").WithForeground(theme.Foreground).Bold().WidthPercent(35),
            new TextNode("Installed").WithForeground(theme.Foreground).Bold().WidthPercent(15),
            new TextNode("Available").WithForeground(theme.Foreground).Bold().WidthPercent(15),
            new TextNode("Policy").WithForeground(theme.Foreground).Bold().WidthPercent(15),
            new TextNode("Status").WithForeground(theme.Foreground).Bold().WidthPercent(20)
        ).Height(1);

        var separator = new TextNode(new string('─', 60)).WithForeground(theme.Border).Height(1);
        var rows = new List<ILayoutNode> { header, separator, Layouts.Empty().Height(1) };

        for (var i = 0; i < plugins.Count; i++)
        {
            var plugin = plugins[i];
            var isSelected = i == ViewModel.SelectedIndex.Value;
            var marker = isSelected ? "▸" : " ";
            var versionColor = plugin.Status == PluginStatus.UpdateAvailable ? theme.Warning : theme.TextDim;

            rows.Add(Layouts.Horizontal(
                new TextNode($"{marker} {plugin.Name}")
                    .WithForeground(isSelected ? theme.Foreground : theme.TextDim).WidthPercent(35),
                new TextNode(plugin.InstalledVersion ?? "")
                    .WithForeground(theme.TextDim).WidthPercent(15),
                new TextNode(plugin.AvailableVersion)
                    .WithForeground(versionColor).WidthPercent(15),
                new TextNode(plugin.UpdatePolicy?.ToString().ToLowerInvariant() ?? "auto")
                    .WithForeground(theme.TextDim).WidthPercent(15),
                new TextNode($"{GetStatusIcon(plugin)} {GetStatusLabel(plugin)}")
                    .WithForeground(GetStatusColor(plugin, theme)).WidthPercent(20)
            ).Height(1));
        }

        var tablePanel = new PanelNode()
            .WithBorder(BorderStyle.Rounded)
            .WithBorderColor(theme.Accent)
            .WithTitle("📋 Installed Plugins")
            .WithTitleColor(theme.PanelTitle)
            .WithPadding(1)
            .WithContent(Layouts.Vertical(rows.ToArray()));

        var selected = ViewModel.SelectedPlugin.Value;
        if (selected is null)
            return tablePanel;

        var detailRows = new List<ILayoutNode>
        {
            Layouts.Horizontal(
                new TextNode(selected.Name).WithForeground(theme.Foreground).Bold().WidthAuto(),
                new TextNode("  ").WidthAuto(),
                GetActionBadgeNode(selected, theme)
            ).Height(1),
            Layouts.Empty().Height(1),
            new KeyValueRowNode("Author", selected.Author, labelWidth: 12, labelColor: theme.TextDim, valueColor: theme.Foreground),
            new KeyValueRowNode("Tags", string.Join(", ", selected.Tags), labelWidth: 12, labelColor: theme.TextDim, valueColor: theme.Foreground),
            new KeyValueRowNode("Delivery", selected.Delivery.Type.ToString(), labelWidth: 12, labelColor: theme.TextDim, valueColor: theme.Foreground),
        };

        var detailPanel = new PanelNode()
            .WithBorder(BorderStyle.Rounded)
            .WithBorderColor(theme.Accent)
            .WithTitle("ℹ Details")
            .WithTitleColor(theme.PanelTitle)
            .WithPadding(1)
            .WithContent(Layouts.Vertical(detailRows.ToArray()));

        return Layouts.Vertical(tablePanel, detailPanel);
    }

    private static string GetStatusIcon(PluginInfo plugin) => plugin switch
    {
        { UpdatePolicy: UpdatePolicy.Pinned } => "⏸",
        { Status: PluginStatus.UpdateAvailable } => "↑",
        _ => "✓"
    };

    private static Color GetStatusColor(PluginInfo plugin, ThemeDefinition theme) => plugin switch
    {
        { UpdatePolicy: UpdatePolicy.Pinned } => theme.TextDim,
        { Status: PluginStatus.UpdateAvailable } => theme.Warning,
        _ => theme.Success
    };

    private static string GetStatusLabel(PluginInfo plugin) => plugin switch
    {
        { UpdatePolicy: UpdatePolicy.Pinned } => "pinned",
        { Status: PluginStatus.UpdateAvailable } => "update",
        _ => "current"
    };

    // --- Sources View ---

    private LayoutNode BuildSourcesView()
    {
        var theme = _themeService.Current;
        var sources = ViewModel.Sources.Value;

        var registryRows = new List<ILayoutNode>();
        if (sources.Registries.Count == 0)
        {
            registryRows.Add(new TextNode("(none)").WithForeground(theme.TextDim).Height(1));
        }
        else
        {
            foreach (var url in sources.Registries)
            {
                registryRows.Add(Layouts.Horizontal(
                    new TextNode("● ").WithForeground(theme.Success).WidthAuto(),
                    new TextNode(url).WithForeground(theme.Foreground).Bold().WidthFill()
                ).Height(1));
                registryRows.Add(Layouts.Horizontal(
                    new TextNode("  ").WidthAuto(),
                    new BadgeNode("CONNECTED", theme.SelectionText, theme.Success).Height(1)
                ).Height(1));
                registryRows.Add(Layouts.Empty().Height(1));
            }
        }

        var registriesPanel = new PanelNode()
            .WithBorder(BorderStyle.Rounded)
            .WithBorderColor(theme.Accent)
            .WithTitle("🌐 Registries")
            .WithTitleColor(theme.PanelTitle)
            .WithPadding(1)
            .WithContent(Layouts.Vertical(registryRows.ToArray()));

        var repoRows = new List<ILayoutNode>();
        if (sources.Repositories.Count == 0)
        {
            repoRows.Add(new TextNode("(none)").WithForeground(theme.TextDim).Height(1));
        }
        else
        {
            foreach (var path in sources.Repositories)
            {
                repoRows.Add(Layouts.Horizontal(
                    new TextNode("○ ").WithForeground(theme.TextDim).WidthAuto(),
                    new TextNode(path).WithForeground(theme.Foreground).WidthFill()
                ).Height(1));
                repoRows.Add(Layouts.Horizontal(
                    new TextNode("  ").WidthAuto(),
                    new BadgeNode("LOCAL", theme.Foreground, theme.TextDim).Height(1)
                ).Height(1));
                repoRows.Add(Layouts.Empty().Height(1));
            }
        }

        var reposPanel = new PanelNode()
            .WithBorder(BorderStyle.Rounded)
            .WithBorderColor(theme.Accent)
            .WithTitle("📁 Manual Repositories")
            .WithTitleColor(theme.PanelTitle)
            .WithPadding(1)
            .WithContent(Layouts.Vertical(repoRows.ToArray()));

        var actionsContent = Layouts.Horizontal(
            new TextNode("Press ").WithForeground(theme.TextDim).WidthAuto(),
            new TextNode("a").WithForeground(theme.Foreground).Bold().WidthAuto(),
            new TextNode(" to add registry  │  Press ").WithForeground(theme.TextDim).WidthAuto(),
            new TextNode("r").WithForeground(theme.Foreground).Bold().WidthAuto(),
            new TextNode(" to refresh all").WithForeground(theme.TextDim).WidthFill()
        );

        var actionsPanel = new PanelNode()
            .WithBorder(BorderStyle.Rounded)
            .WithBorderColor(theme.Accent)
            .WithTitle("＋ Actions")
            .WithTitleColor(theme.PanelTitle)
            .WithPadding(1)
            .WithContent(actionsContent);

        return Layouts.Vertical(registriesPanel, reposPanel, actionsPanel);
    }
}
