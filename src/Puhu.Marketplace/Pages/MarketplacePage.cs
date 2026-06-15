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
    private ModalNode? _activeModal;
    private bool _showModal;

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

        LayoutNode activeView = ViewModel.ActiveView.Value switch
        {
            MarketplaceView.Browse => BuildBrowseView(),
            MarketplaceView.Installed => BuildInstalledView(),
            MarketplaceView.Sources => BuildSourcesView(),
            _ => Layouts.Empty()
        };

        var main = Layouts.Vertical(
            _subNav!.Height(1),
            new SubNavSeparatorNode(theme.Accent).Height(1),
            activeView.Fill());

        if (_showModal && _activeModal is not null)
            return Layouts.Stack(main, _activeModal);

        return main;
    }

    public override void OnNavigatedTo()
    {
        base.OnNavigatedTo();
        RegisterPageKeys();

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

        ViewModel.SourceSelectedIndex
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

    private void RegisterPageKeys()
    {
        KeyBindings.Clear();

        // SubNav keys need to be re-registered after Clear since OnBound registered them
        _subNav = new SubNavNode<MarketplaceView>(
            ViewModel.ActiveView,
            KeyBindings,
            _themeService,
            (ConsoleKey.D1, "Browse", MarketplaceView.Browse),
            (ConsoleKey.D2, "Installed", MarketplaceView.Installed),
            (ConsoleKey.D3, "Sources", MarketplaceView.Sources));

        KeyBindings.RegisterGlobalKeys(
            () => ViewModel.RequestShutdown(),
            path => Navigate(path),
            _tabNavigator,
            _refreshController);

        KeyBindings.Register(ConsoleKey.UpArrow, () =>
        {
            if (ViewModel.ActiveView.Value == MarketplaceView.Sources)
                ViewModel.MoveSourceSelection(-1);
            else
                ViewModel.MoveSelection(-1);
            InvalidateLayout();
        });
        KeyBindings.Register(ConsoleKey.DownArrow, () =>
        {
            if (ViewModel.ActiveView.Value == MarketplaceView.Sources)
                ViewModel.MoveSourceSelection(1);
            else
                ViewModel.MoveSelection(1);
            InvalidateLayout();
        });

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

        KeyBindings.Register(ConsoleKey.I, () =>
        {
            if (ViewModel.ActiveView.Value == MarketplaceView.Browse)
                ViewModel.HandleAction();
        });

        KeyBindings.Register(ConsoleKey.R, () => ViewModel.Refresh());

        KeyBindings.Register(ConsoleKey.U, () =>
        {
            if (ViewModel.ActiveView.Value == MarketplaceView.Installed)
                ViewModel.UpdateSelected();
        });

        KeyBindings.Register(ConsoleKey.X, () =>
        {
            if (ViewModel.ActiveView.Value == MarketplaceView.Installed)
                ViewModel.UninstallSelected();
            else if (ViewModel.ActiveView.Value == MarketplaceView.Sources)
                ShowRemoveSourceModal();
        });

        KeyBindings.Register(ConsoleKey.C, () =>
        {
            if (ViewModel.ActiveView.Value == MarketplaceView.Installed &&
                ViewModel.SelectedPlugin.Value is { } plugin)
                ViewModel.CyclePolicy(plugin.Id);
        });

        KeyBindings.Register(ConsoleKey.A, () =>
        {
            if (ViewModel.ActiveView.Value == MarketplaceView.Sources)
                ShowAddSourceModal();
        });
    }

    private void RegisterModalKeys()
    {
        KeyBindings.Clear();
        KeyBindings.Register(ConsoleKey.Escape, DismissModal);
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
        var selectedIndex = ViewModel.SourceSelectedIndex.Value;
        var flatIndex = 0;

        var registryRows = new List<ILayoutNode>();
        if (sources.Registries.Count == 0)
        {
            registryRows.Add(new TextNode("(none)").WithForeground(theme.TextDim).Height(1));
        }
        else
        {
            foreach (var url in sources.Registries)
            {
                var isSelected = flatIndex == selectedIndex;
                var marker = isSelected ? "▸" : " ";
                registryRows.Add(Layouts.Horizontal(
                    new TextNode($"{marker} ● ").WithForeground(isSelected ? theme.Foreground : theme.Success).WidthAuto(),
                    new TextNode(url).WithForeground(isSelected ? theme.Foreground : theme.TextDim).Bold().WidthFill()
                ).Height(1));
                registryRows.Add(Layouts.Horizontal(
                    new TextNode("    ").WidthAuto(),
                    new BadgeNode("CONNECTED", theme.SelectionText, theme.Success).Height(1)
                ).Height(1));
                registryRows.Add(Layouts.Empty().Height(1));
                flatIndex++;
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
                var isSelected = flatIndex == selectedIndex;
                var marker = isSelected ? "▸" : " ";
                repoRows.Add(Layouts.Horizontal(
                    new TextNode($"{marker} ○ ").WithForeground(isSelected ? theme.Foreground : theme.TextDim).WidthAuto(),
                    new TextNode(path).WithForeground(isSelected ? theme.Foreground : theme.TextDim).WidthFill()
                ).Height(1));
                repoRows.Add(Layouts.Horizontal(
                    new TextNode("    ").WidthAuto(),
                    new BadgeNode("LOCAL", theme.Foreground, theme.TextDim).Height(1)
                ).Height(1));
                repoRows.Add(Layouts.Empty().Height(1));
                flatIndex++;
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
            new TextNode(" to add source  │  Press ").WithForeground(theme.TextDim).WidthAuto(),
            new TextNode("x").WithForeground(theme.Foreground).Bold().WidthAuto(),
            new TextNode(" to remove selected  │  Press ").WithForeground(theme.TextDim).WidthAuto(),
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

    // --- Modals ---

    private void ShowAddSourceModal()
    {
        var theme = _themeService.Current;
        var typeList = new SelectionListNode<string>(
            ["Registry (URL)", "Repository (Folder)"], item => item)
            .WithHighlightColors(theme.SelectionText, theme.Selection)
            .WithForeground(theme.Foreground)
            .WithVisibleRows(2);

        _activeModal = new ModalNode()
            .WithTitle("Add Source")
            .WithBorder(BorderStyle.Rounded)
            .WithBorderColor(theme.Accent)
            .WithBackdrop(BackdropStyle.Dim)
            .WithPadding(1)
            .WithDismissOnEscape(false)
            .WithContent(typeList);

        typeList.SelectionConfirmed.Subscribe(selected =>
        {
            var choice = selected[0];
            if (choice.StartsWith("Registry"))
                ShowUrlInputModal();
            else
                ShowFolderPickerModal();
        }).DisposeWith(Subscriptions);

        typeList.Cancelled.Subscribe(_ => DismissModal()).DisposeWith(Subscriptions);

        OpenModal();
    }

    private void ShowUrlInputModal()
    {
        Focus.PopFocus();

        var theme = _themeService.Current;
        var input = new TextInputNode()
            .WithPlaceholder("https://registry.example.com")
            .WithForeground(theme.Foreground);

        _activeModal = new ModalNode()
            .WithTitle("Add Registry URL")
            .WithBorder(BorderStyle.Rounded)
            .WithBorderColor(theme.Accent)
            .WithBackdrop(BackdropStyle.Dim)
            .WithPadding(1)
            .WithDismissOnEscape(false)
            .WithContent(input);

        input.Submitted.Subscribe(url =>
        {
            if (!string.IsNullOrWhiteSpace(url))
                ViewModel.AddSource(url.Trim(), SourceType.Registry);
            DismissModal();
        }).DisposeWith(Subscriptions);

        InvalidateLayout();
        Focus.PushFocus(_activeModal);
    }

    private void ShowFolderPickerModal()
    {
        Focus.PopFocus();

        var theme = _themeService.Current;
        var picker = new FilePickerNode()
            .WithMode(FilePickerMode.Directories)
            .WithHighlightColors(theme.SelectionText, theme.Selection)
            .WithDirectoryColor(theme.Accent)
            .WithFillHeight(true);

        _activeModal = new ModalNode()
            .WithTitle("Select Repository Folder")
            .WithBorder(BorderStyle.Rounded)
            .WithBorderColor(theme.Accent)
            .WithBackdrop(BackdropStyle.Dim)
            .WithPadding(1)
            .WithDismissOnEscape(false)
            .WithContent(picker);

        picker.SelectionConfirmed.Subscribe(selected =>
        {
            if (selected.Count > 0)
                ViewModel.AddSource(selected[0], SourceType.Repository);
            DismissModal();
        }).DisposeWith(Subscriptions);

        picker.Cancelled.Subscribe(_ => DismissModal()).DisposeWith(Subscriptions);

        InvalidateLayout();
        Focus.PushFocus(_activeModal);
    }

    private void ShowRemoveSourceModal()
    {
        var url = ViewModel.SelectedSourceUrl;
        if (url is null) return;

        var theme = _themeService.Current;
        var confirmList = new SelectionListNode<string>(
            ["Confirm", "Cancel"], item => item)
            .WithHighlightColors(theme.SelectionText, theme.Selection)
            .WithForeground(theme.Foreground)
            .WithVisibleRows(2);

        var content = Layouts.Vertical(
            new TextNode("Remove source?").WithForeground(theme.Foreground).Height(1),
            new TextNode(url).WithForeground(theme.TextDim).Height(1),
            Layouts.Empty().Height(1),
            confirmList
        );

        _activeModal = new ModalNode()
            .WithTitle("Remove Source")
            .WithBorder(BorderStyle.Rounded)
            .WithBorderColor(theme.Error)
            .WithBackdrop(BackdropStyle.Dim)
            .WithPadding(1)
            .WithDismissOnEscape(false)
            .WithContent(content);

        confirmList.SelectionConfirmed.Subscribe(selected =>
        {
            if (selected[0] == "Confirm")
                ViewModel.RemoveSelectedSource();
            DismissModal();
        }).DisposeWith(Subscriptions);

        confirmList.Cancelled.Subscribe(_ => DismissModal()).DisposeWith(Subscriptions);

        OpenModal();
    }

    private void OpenModal()
    {
        _showModal = true;
        RegisterModalKeys();
        InvalidateLayout();
        Focus.PushFocus(_activeModal!);
    }

    private void DismissModal()
    {
        if (!_showModal) return;
        Focus.PopFocus();
        _activeModal = null;
        _showModal = false;
        RegisterPageKeys();
        InvalidateLayout();
    }
}
