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

public sealed class MarketplacePage : ReactivePage<MarketplaceViewModel>
{
    private readonly IToastService _toastService;
    private readonly IThemeService _themeService;
    private KeyedDynamicLayoutNode<MarketplaceView>? _viewSwitcher;
    private int _expandedIndex = -1;

    public MarketplacePage(IToastService toastService, IThemeService themeService)
    {
        _toastService = toastService;
        _themeService = themeService;
        FocusPolicy = FocusPolicy.FirstFocusable;
    }

    protected override void OnBound()
    {
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

    public override ILayoutNode BuildLayout()
    {
        var hints = ViewModel.ActiveView.Value switch
        {
            MarketplaceView.Browse => new[] { "↑↓:Navigate", "Enter:Expand", "i:Install", "r:Refresh" },
            MarketplaceView.Installed => new[] { "↑↓:Navigate", "u:Update", "x:Uninstall", "p:Policy" },
            MarketplaceView.Sources => new[] { "↑↓:Navigate", "a:Add", "x:Remove" },
            _ => Array.Empty<string>()
        };

        return AppShell.Wrap(_themeService.Current, 0, _viewSwitcher!.Fill(), hints);
    }

    public override void OnNavigatedTo()
    {
        base.OnNavigatedTo();

        // View switching
        KeyBindings.Register(ConsoleKey.D1, () => ViewModel.SwitchView(MarketplaceView.Browse));
        KeyBindings.Register(ConsoleKey.D2, () => ViewModel.SwitchView(MarketplaceView.Installed));
        KeyBindings.Register(ConsoleKey.D3, () => ViewModel.SwitchView(MarketplaceView.Sources));

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
                    ? -1 : ViewModel.SelectedIndex.Value;
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
        KeyBindings.Register(ConsoleKey.P, () =>
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
                var isError = msg!.StartsWith("Error:");
                _toastService.Show(msg, new ToastOptions(
                    Color: isError ? Color.Red : Color.Green,
                    Icon: isError ? "✗" : "✓"));
            })
            .DisposeWith(Subscriptions);
    }

    // --- Browse View ---

    private ILayoutNode BuildBrowseView()
    {
        var plugins = ViewModel.AvailablePlugins.Value;
        if (plugins.Count == 0)
            return new TextNode("No plugins available. Press r to refresh.").WithForeground(Color.DarkGray);

        var rows = new List<ILayoutNode>();
        for (var i = 0; i < plugins.Count; i++)
        {
            var plugin = plugins[i];
            var isSelected = i == ViewModel.SelectedIndex.Value;
            var isExpanded = i == _expandedIndex;
            rows.Add(BuildBrowseRow(plugin, isSelected, isExpanded));
        }

        return Layouts.Vertical(rows.ToArray()).Fill();
    }

    private ILayoutNode BuildBrowseRow(PluginInfo plugin, bool isSelected, bool isExpanded)
    {
        var marker = isExpanded ? "▾" : isSelected ? "▸" : " ";
        var activeOp = ViewModel.ActiveOperations.Value.GetValueOrDefault(plugin.Id);

        var header = Layouts.Horizontal(
            new TextNode($"{marker} {plugin.Name}")
                .WithForeground(isSelected ? Color.White : Color.Gray).WidthFill(),
            new TextNode(plugin.AvailableVersion).WithForeground(Color.DarkGray),
            new TextNode($"  @{plugin.Author}").WithForeground(Color.DarkGray),
            activeOp is not null
                ? new SpinnerNode()
                    .WithLabel(activeOp).WithSpinnerColor(Color.Green)
                : new TextNode($"  {GetActionBadge(plugin)}")
                    .WithForeground(GetBadgeColor(plugin.Status))
        ).Height(1);

        if (!isExpanded) return header;

        var detail = Layouts.Vertical(
            new TextNode(plugin.Description).WithForeground(Color.Gray),
            new TextNode($"Tags: {string.Join(", ", plugin.Tags)}").WithForeground(Color.DarkGray),
            new TextNode($"Delivery: {plugin.Delivery.Type}").WithForeground(Color.DarkGray)
        );

        var panel = new PanelNode()
            .WithBorder(BorderStyle.Rounded)
            .WithBorderColor(Color.DarkGray)
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

    private static Color GetBadgeColor(PluginStatus status) => status switch
    {
        PluginStatus.NotInstalled => Color.Blue,
        PluginStatus.Installed => Color.Green,
        PluginStatus.UpdateAvailable => Color.Yellow,
        _ => Color.Gray
    };

    // --- Installed View ---

    private ILayoutNode BuildInstalledView()
    {
        var plugins = ViewModel.InstalledPlugins.Value;
        if (plugins.Count == 0)
            return new TextNode("No plugins installed.").WithForeground(Color.DarkGray);

        var rows = new List<ILayoutNode>();

        rows.Add(Layouts.Horizontal(
            new TextNode("Plugin").WithForeground(Color.White).Bold().WidthPercent(35),
            new TextNode("Installed").WithForeground(Color.White).Bold().WidthPercent(15),
            new TextNode("Available").WithForeground(Color.White).Bold().WidthPercent(15),
            new TextNode("Policy").WithForeground(Color.White).Bold().WidthPercent(15),
            new TextNode("Status").WithForeground(Color.White).Bold().WidthPercent(20)
        ).Height(1));

        rows.Add(new TextNode(new string('─', 60)).WithForeground(Color.DarkGray).Height(1));

        for (var i = 0; i < plugins.Count; i++)
        {
            var plugin = plugins[i];
            var isSelected = i == ViewModel.SelectedIndex.Value;
            var marker = isSelected ? "▸" : " ";

            rows.Add(Layouts.Horizontal(
                new TextNode($"{marker} {plugin.Name}")
                    .WithForeground(isSelected ? Color.White : Color.Gray).WidthPercent(35),
                new TextNode(plugin.InstalledVersion ?? "")
                    .WithForeground(Color.Gray).WidthPercent(15),
                new TextNode(plugin.AvailableVersion)
                    .WithForeground(Color.Gray).WidthPercent(15),
                new TextNode(plugin.UpdatePolicy?.ToString() ?? "Auto")
                    .WithForeground(Color.Gray).WidthPercent(15),
                new TextNode(GetStatusIcon(plugin))
                    .WithForeground(GetStatusColor(plugin)).WidthPercent(20)
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

    private static Color GetStatusColor(PluginInfo plugin) => plugin switch
    {
        { UpdatePolicy: UpdatePolicy.Pinned } => Color.DarkGray,
        { Status: PluginStatus.UpdateAvailable } => Color.Yellow,
        _ => Color.Green
    };

    // --- Sources View ---

    private ILayoutNode BuildSourcesView()
    {
        var sources = ViewModel.Sources.Value;
        var rows = new List<ILayoutNode>();

        rows.Add(new TextNode("Registries").WithForeground(Color.DarkGray).Bold().Height(1));
        if (sources.Registries.Count == 0)
        {
            rows.Add(new TextNode("  (none)").WithForeground(Color.DarkGray).Height(1));
        }
        else
        {
            foreach (var url in sources.Registries)
                rows.Add(new TextNode($"  {url}").WithForeground(Color.Gray).Height(1));
        }

        rows.Add(Layouts.Empty().Height(1));

        rows.Add(new TextNode("Manual Repositories").WithForeground(Color.DarkGray).Bold().Height(1));
        if (sources.Repositories.Count == 0)
        {
            rows.Add(new TextNode("  (none)").WithForeground(Color.DarkGray).Height(1));
        }
        else
        {
            foreach (var url in sources.Repositories)
                rows.Add(new TextNode($"  {url}").WithForeground(Color.Gray).Height(1));
        }

        return Layouts.Vertical(rows.ToArray()).Fill();
    }

}
