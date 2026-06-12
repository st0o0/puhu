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
    private KeyedDynamicLayoutNode<MarketplaceView>? _viewSwitcher;
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

        _viewSwitcher = Layouts.KeyedDynamic(
            () => ViewModel.ActiveView.Value,
            view => view switch
            {
                MarketplaceView.Browse => BuildBrowseView(),
                MarketplaceView.Installed => BuildInstalledView(),
                MarketplaceView.Sources => BuildSourcesView(),
                _ => Layouts.Empty()
            });
    }

    public string[] GetKeyHints() => ViewModel.ActiveView.Value switch
    {
        MarketplaceView.Browse => ["↑↓:Navigate", "Enter:Expand", "i:Install", "r:Refresh", "Esc:Quit", "Tab:Switch"],
        MarketplaceView.Installed => ["↑↓:Navigate", "u:Update", "x:Uninstall", "c:Policy", "Esc:Quit", "Tab:Switch"],
        MarketplaceView.Sources => ["↑↓:Navigate", "a:Add", "x:Remove", "Esc:Quit", "Tab:Switch"],
        _ => []
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
            .Subscribe(_ =>
            {
                _viewSwitcher?.Invalidate();
                InvalidateLayout();
            })
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

    private ILayoutNode BuildBrowseView()
    {
        var theme = _themeService.Current;
        var plugins = ViewModel.AvailablePlugins.Value;
        if (plugins.Count == 0)
            return new TextNode("No plugins available. Press r to refresh.").WithForeground(theme.TextDim);

        var rows = new List<ILayoutNode>();
        for (var i = 0; i < plugins.Count; i++)
        {
            var plugin = plugins[i];
            var isSelected = i == ViewModel.SelectedIndex.Value;
            var isExpanded = i == _expandedIndex;
            rows.Add(BuildBrowseRow(plugin, isSelected, isExpanded, theme));
        }

        return Layouts.Vertical(rows.ToArray()).Fill();
    }

    private ILayoutNode BuildBrowseRow(PluginInfo plugin, bool isSelected, bool isExpanded, ThemeDefinition theme)
    {
        var marker = isExpanded ? "▾" : isSelected ? "▸" : " ";
        var activeOp = ViewModel.ActiveOperations.Value.GetValueOrDefault(plugin.Id);

        var header = Layouts.Horizontal(
            new TextNode($"{marker} {plugin.Name}")
                .WithForeground(isSelected ? theme.Foreground : theme.TextDim).WidthFill(),
            new TextNode(plugin.AvailableVersion).WithForeground(theme.TextDim),
            new TextNode($"  @{plugin.Author}").WithForeground(theme.TextDim),
            activeOp is not null
                ? new SpinnerNode()
                    .WithLabel(activeOp).WithSpinnerColor(theme.Success)
                : new TextNode($"  {GetActionBadge(plugin)}")
                    .WithForeground(GetBadgeColor(plugin.Status, theme))
        ).Height(1);

        if (!isExpanded) return header;

        var detail = Layouts.Vertical(
            new TextNode(plugin.Description).WithForeground(theme.TextDim),
            new TextNode($"Tags: {string.Join(", ", plugin.Tags)}").WithForeground(theme.TextDim),
            new TextNode($"Delivery: {plugin.Delivery.Type}").WithForeground(theme.TextDim)
        );

        var panel = new PanelNode()
            .WithBorder(BorderStyle.Rounded)
            .WithBorderColor(theme.Border)
            .WithContent(detail);

        return Layouts.Vertical(header, panel);
    }

    private static string GetActionBadge(PluginInfo plugin) => plugin.Status switch
    {
        PluginStatus.NotInstalled => "[Install]",
        PluginStatus.Installed => "[Installed]",
        PluginStatus.UpdateAvailable => "[Update]",
        _ => ""
    };

    private static Color GetBadgeColor(PluginStatus status, ThemeDefinition theme) => status switch
    {
        PluginStatus.NotInstalled => theme.Accent,
        PluginStatus.Installed => theme.Success,
        PluginStatus.UpdateAvailable => theme.Warning,
        _ => theme.TextDim
    };

    // --- Installed View ---

    private ILayoutNode BuildInstalledView()
    {
        var theme = _themeService.Current;
        var plugins = ViewModel.InstalledPlugins.Value;
        if (plugins.Count == 0)
            return new TextNode("No plugins installed.").WithForeground(theme.TextDim);

        var rows = new List<ILayoutNode>
        {
            Layouts.Horizontal(
                new TextNode("Plugin").WithForeground(theme.Foreground).Bold().WidthPercent(35),
                new TextNode("Installed").WithForeground(theme.Foreground).Bold().WidthPercent(15),
                new TextNode("Available").WithForeground(theme.Foreground).Bold().WidthPercent(15),
                new TextNode("Policy").WithForeground(theme.Foreground).Bold().WidthPercent(15),
                new TextNode("Status").WithForeground(theme.Foreground).Bold().WidthPercent(20)
            ).Height(1),
            new TextNode(new string('─', 60)).WithForeground(theme.Border).Height(1)
        };

        for (var i = 0; i < plugins.Count; i++)
        {
            var plugin = plugins[i];
            var isSelected = i == ViewModel.SelectedIndex.Value;
            var marker = isSelected ? "▸" : " ";

            rows.Add(Layouts.Horizontal(
                new TextNode($"{marker} {plugin.Name}")
                    .WithForeground(isSelected ? theme.Foreground : theme.TextDim).WidthPercent(35),
                new TextNode(plugin.InstalledVersion ?? "")
                    .WithForeground(theme.TextDim).WidthPercent(15),
                new TextNode(plugin.AvailableVersion)
                    .WithForeground(theme.TextDim).WidthPercent(15),
                new TextNode(plugin.UpdatePolicy?.ToString() ?? "Auto")
                    .WithForeground(theme.TextDim).WidthPercent(15),
                new TextNode(GetStatusIcon(plugin))
                    .WithForeground(GetStatusColor(plugin, theme)).WidthPercent(20)
            ).Height(1));
        }

        return Layouts.Vertical(rows.ToArray()).Fill();
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

    // --- Sources View ---

    private ILayoutNode BuildSourcesView()
    {
        var theme = _themeService.Current;
        var sources = ViewModel.Sources.Value;
        var rows = new List<ILayoutNode> { new TextNode("Registries").WithForeground(theme.TextDim).Bold().Height(1) };

        if (sources.Registries.Count == 0)
        {
            rows.Add(new TextNode("  (none)").WithForeground(theme.TextDim).Height(1));
        }
        else
        {
            foreach (var url in sources.Registries)
            {
                rows.Add(new TextNode($"  {url}").WithForeground(theme.TextDim).Height(1));
            }
        }

        rows.Add(Layouts.Empty().Height(1));

        rows.Add(new TextNode("Manual Repositories").WithForeground(theme.TextDim).Bold().Height(1));
        if (sources.Repositories.Count == 0)
        {
            rows.Add(new TextNode("  (none)").WithForeground(theme.TextDim).Height(1));
        }
        else
        {
            foreach (var url in sources.Repositories)
            {
                rows.Add(new TextNode($"  {url}").WithForeground(theme.TextDim).Height(1));
            }
        }

        return Layouts.Vertical(rows.ToArray()).Fill();
    }
}
