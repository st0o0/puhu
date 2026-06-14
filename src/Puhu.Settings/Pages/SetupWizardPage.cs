using Puhu.Plugin;
using R3;
using Termina.Input;
using Termina.Layout;
using Termina.Reactive;
using Termina.Rendering;

namespace Puhu.Settings.Pages;

/// <summary>
/// Full-screen first-run wizard. Deliberately does NOT implement
/// IKeyHintProvider, so the shell decorator leaves it chromeless.
/// </summary>
public sealed class SetupWizardPage : ReactivePage<SetupWizardViewModel>
{
    private readonly IThemeService _themeService;

    private WizardNode<SetupStep>? _wizard;
    private DynamicLayoutNode? _themeStep;
    private DynamicLayoutNode? _refreshStep;
    private DynamicLayoutNode? _tabStep;
    private DynamicLayoutNode? _pluginStep;
    private DynamicLayoutNode? _doneStep;

    public SetupWizardPage(IThemeService themeService)
    {
        _themeService = themeService;
        FocusPolicy = Termina.Input.FocusPolicy.ByPriority;
    }

    protected override void OnBound()
    {
        _themeStep = new DynamicLayoutNode(() => WizardSteps.Theme(
            ViewModel.Themes, ViewModel.ThemeIndex, _themeService.CurrentThemeName, _themeService.Current));
        _refreshStep = new DynamicLayoutNode(() => Layouts.Vertical(
            SettingsRows.RefreshRows(ViewModel.RefreshSteps, ViewModel.RefreshIndex, _themeService.Current).ToArray()));
        _tabStep = new DynamicLayoutNode(() => Layouts.Vertical(BuildTabRows()));
        _pluginStep = new DynamicLayoutNode(() => Layouts.Vertical(BuildPluginRows()));
        _doneStep = new DynamicLayoutNode(BuildDone);

        _wizard = new WizardNode<SetupStep>()
            .WithProgressStyle(WizardProgressStyle.None)
            .WithStep(SetupStep.Welcome, "Welcome", () => WizardSteps.Welcome(_themeService.Current), helpText: null)
            .WithStep(SetupStep.Theme, "Theme", () => _themeStep!, helpText: null)
            .WithStep(SetupStep.Refresh, "Refresh", () => _refreshStep!, helpText: null)
            .WithStep(SetupStep.TabOrder, "Tabs", () => _tabStep!, helpText: null)
            .WithStep(SetupStep.Plugins, "Plugins", () => _pluginStep!, helpText: null)
            .WithStep(SetupStep.Done, "Done", () => _doneStep!, helpText: null);
    }

    public override ILayoutNode BuildLayout()
    {
        var theme = _themeService.Current;
        var step = _wizard!.CurrentStep;

        var breadcrumb = (LayoutNode)WizardChrome.Breadcrumb(step, theme);
        var statusBar = (LayoutNode)WizardChrome.StatusBar((int)step, WizardChrome.StepCount, WizardChrome.HintsFor(step), theme);

        var content = Layouts.Vertical(
            breadcrumb.Height(1),
            new RuleNode(theme.Border).Height(1),
            _wizard!.Fill(),
            new RuleNode(theme.Border).Height(1),
            statusBar.Height(1));

        return new PanelNode()
            .WithBorder(BorderStyle.Rounded)
            .WithBorderColor(theme.Border)
            .WithTitle(" Puhu Setup ")
            .WithTitleColor(theme.PanelTitle)
            .WithContent(content);
    }

    public override void OnNavigatedTo()
    {
        base.OnNavigatedTo();

        _wizard!.Completed.Subscribe(_ => ViewModel.Finish()).DisposeWith(Subscriptions);
        _wizard!.StepChanged.Subscribe(step =>
        {
            if (step == SetupStep.Done)
            {
                _doneStep!.Invalidate();
            }

            InvalidateLayout();
        }).DisposeWith(Subscriptions);

        KeyBindings.Register(ConsoleKey.UpArrow, () => OnArrow(-1));
        KeyBindings.Register(ConsoleKey.DownArrow, () => OnArrow(1));
        KeyBindings.Register(ConsoleKey.UpArrow, ConsoleModifiers.Shift, () => OnShiftArrow(-1));
        KeyBindings.Register(ConsoleKey.DownArrow, ConsoleModifiers.Shift, () => OnShiftArrow(1));
        KeyBindings.Register(ConsoleKey.S, () => ViewModel.Skip());
        KeyBindings.Register(ConsoleKey.O, () =>
        {
            if (_wizard!.CurrentStep == SetupStep.Plugins)
            {
                ViewModel.ToggleOpenMarketplace();
                _pluginStep!.Invalidate();
                ViewModel.RequestRedraw();
            }
        });
    }

    private void OnArrow(int delta)
    {
        switch (_wizard!.CurrentStep)
        {
            case SetupStep.Theme:
                ViewModel.MoveTheme(delta);
                _themeStep!.Invalidate();
                break;
            case SetupStep.Refresh:
                ViewModel.MoveRefresh(delta);
                _refreshStep!.Invalidate();
                break;
            case SetupStep.TabOrder:
                ViewModel.MoveTabSelection(delta);
                _tabStep!.Invalidate();
                break;
            default:
                return;
        }

        ViewModel.RequestRedraw();
    }

    private void OnShiftArrow(int delta)
    {
        if (_wizard!.CurrentStep == SetupStep.TabOrder)
        {
            ViewModel.MoveTab(delta);
            _tabStep!.Invalidate();
            ViewModel.RequestRedraw();
        }
    }

    private ILayoutNode BuildDone()
    {
        var themeName = ViewModel.Themes.Count > 0 ? ViewModel.Themes[ViewModel.ThemeIndex] : "default";
        var refresh = IntervalFormat.Format(ViewModel.RefreshSteps[ViewModel.RefreshIndex]);
        return WizardSteps.Done(_themeService.Current, themeName, refresh);
    }

    private ILayoutNode[] BuildTabRows()
    {
        var theme = _themeService.Current;
        var rows = new List<ILayoutNode>();
        for (var i = 0; i < ViewModel.Tabs.Count; i++)
        {
            var isSelected = i == ViewModel.TabIndex;
            var marker = isSelected ? "▸" : " ";
            rows.Add(new TextNode($"{marker} {ViewModel.Tabs[i].Label.ToLowerInvariant()}")
                .WithForeground(isSelected ? theme.Foreground : theme.TextDim)
                .Height(1));
        }
        return rows.ToArray();
    }

    private ILayoutNode[] BuildPluginRows()
    {
        var theme = _themeService.Current;
        var marker = ViewModel.OpenMarketplaceAfter ? "[x]" : "[ ]";
        return
        [
            new TextNode("Install plugins from the Marketplace any time.").WithForeground(theme.TextDim).Height(1),
            Layouts.Empty().Height(1),
            new TextNode($"{marker} Open the Marketplace when setup finishes (O)")
                .WithForeground(ViewModel.OpenMarketplaceAfter ? theme.Foreground : theme.TextDim)
                .Height(1),
        ];
    }
}
