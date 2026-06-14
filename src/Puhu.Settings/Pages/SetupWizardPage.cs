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

    public SetupWizardPage(IThemeService themeService)
    {
        _themeService = themeService;
        FocusPolicy = Termina.Input.FocusPolicy.ByPriority;
    }

    protected override void OnBound()
    {
        var borderColor = _themeService.Current.Border;

        _themeStep = new DynamicLayoutNode(() => Layouts.Vertical(
            SettingsRows.ThemeRows(ViewModel.Themes, ViewModel.ThemeIndex, _themeService.CurrentThemeName, _themeService.Current).ToArray()));
        _refreshStep = new DynamicLayoutNode(() => Layouts.Vertical(
            SettingsRows.RefreshRows(ViewModel.RefreshSteps, ViewModel.RefreshIndex, _themeService.Current).ToArray()));
        _tabStep = new DynamicLayoutNode(() => Layouts.Vertical(BuildTabRows()));
        _pluginStep = new DynamicLayoutNode(() => Layouts.Vertical(BuildPluginRows()));

        _wizard = new WizardNode<SetupStep>()
            .WithTitle("Puhu Setup")
            .WithProgressStyle(WizardProgressStyle.BlockBar)
            .WithBorder(BorderStyle.Rounded, borderColor)
            .WithStep(SetupStep.Welcome, "Welcome",
                () => new TextNode("Welcome to Puhu. Let's get you set up.\nEnter: next   Esc: back   S: skip"),
                helpText: "Enter Next · S Skip")
            .WithStep(SetupStep.Theme, "Theme", () => _themeStep!,
                helpText: "↑/↓ choose · Enter next · Esc back · S skip")
            .WithStep(SetupStep.Refresh, "Refresh", () => _refreshStep!,
                helpText: "↑/↓ choose · Enter next · Esc back · S skip")
            .WithStep(SetupStep.TabOrder, "Tabs", () => _tabStep!,
                helpText: "↑/↓ select · ⇧↑/↓ move · Enter next · Esc back · S skip")
            .WithStep(SetupStep.Plugins, "Plugins", () => _pluginStep!,
                helpText: "O toggle open-marketplace · Enter next · Esc back · S skip")
            .WithStep(SetupStep.Done, "Done",
                () => new TextNode("You're all set. Press Enter to finish."),
                helpText: "Enter Finish");
    }

    public override ILayoutNode BuildLayout() => _wizard!;

    public override void OnNavigatedTo()
    {
        base.OnNavigatedTo();

        _wizard!.Completed.Subscribe(_ => ViewModel.Finish()).DisposeWith(Subscriptions);

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
        }
    }

    private void OnShiftArrow(int delta)
    {
        if (_wizard!.CurrentStep == SetupStep.TabOrder)
        {
            ViewModel.MoveTab(delta);
            _tabStep!.Invalidate();
        }
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
